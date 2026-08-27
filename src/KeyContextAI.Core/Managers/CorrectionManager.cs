using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Managers;

/// <summary>
/// Owns the in-memory typing transcript and its privacy lifecycle.
/// </summary>
public sealed class CorrectionManager : IDisposable
{
    private const int MaxTranscriptEntries = 32;

    private readonly IKeystrokeAccessor _keystrokes;
    private readonly IFocusAccessor _focus;
    private readonly IWordAssemblyEngine _wordAssembly;
    private readonly Queue<TranscriptEntry> _transcript = new();
    private readonly object _stateGate = new();
    private PasswordState _passwordState = PasswordState.Unknown;
    private FocusContext? _currentFocus;
    private int _epochId;
    private bool _paused;
    private bool _disposed;

    /// <summary>Creates a manager that starts in the fail-closed state.</summary>
    public CorrectionManager(
        IKeystrokeAccessor keystrokes,
        IFocusAccessor focus,
        IWordAssemblyEngine wordAssembly)
    {
        _keystrokes = keystrokes ?? throw new ArgumentNullException(nameof(keystrokes));
        _focus = focus ?? throw new ArgumentNullException(nameof(focus));
        _wordAssembly = wordAssembly ?? throw new ArgumentNullException(nameof(wordAssembly));

        _keystrokes.KeyObserved += HandleKeyObserved;
        _keystrokes.SequenceGapDetected += HandleSequenceGap;
        _focus.FocusChanged += HandleFocusChanged;
    }

    private void HandleSequenceGap()
    {
        lock (_stateGate)
        {
            if (_disposed)
            {
                return;
            }

            // Keystrokes were dropped, so the word in progress no longer describes the text that
            // reached the application. Continuing from a truncated sequence would let a later
            // correction backspace over the wrong span, so the epoch is invalidated instead.
            InvalidateEpoch();
        }

        // An armed suppression outlives the transaction that armed it unless it is cleared here.
        // The armed state consumes the next committing or separator key wherever it occurs, so
        // leaving it armed across an invalidation withholds an ordinary keystroke the user typed
        // in a context that has nothing to do with the stale transaction.
        _keystrokes.Disarm();
    }

    /// <summary>True when input capture is paused or the password state is not known safe.</summary>
    public bool IsSuspended
    {
        get
        {
            lock (_stateGate)
            {
                return _paused || _passwordState != PasswordState.No;
            }
        }
    }

    /// <summary>Starts observing input after the accessor has been installed.</summary>
    /// <remarks>
    /// The current focus is seeded before the hook is installed, so the first key the user presses
    /// already has a context to be evaluated against. Seeding after installation would leave a
    /// window in which keys are captured and then discarded for want of a focus context.
    /// </remarks>
    public Task StartAsync()
    {
        _focus.PublishCurrentFocus();
        return _keystrokes.InstallAsync();
    }

    /// <summary>Pauses observation and erases all in-memory typing data.</summary>
    public void Pause()
    {
        lock (_stateGate)
        {
            ThrowIfDisposed();
            _paused = true;
            InvalidateEpoch();
        }

        _keystrokes.Disarm();
    }

    /// <summary>Resumes observation only after the current password state is explicitly safe.</summary>
    public void Resume()
    {
        lock (_stateGate)
        {
            ThrowIfDisposed();
            _passwordState = _focus.IsPasswordContext();
            _paused = false;
            if (_passwordState != PasswordState.No)
            {
                WipeTranscript();
                return;
            }
        }
    }

    /// <summary>Stops observation and erases all in-memory typing data.</summary>
    public async Task StopAsync()
    {
        lock (_stateGate)
        {
            ThrowIfDisposed();
            InvalidateEpoch();
            _paused = true;
        }

        _keystrokes.Disarm();
        await _keystrokes.UninstallAsync().ConfigureAwait(false);
    }

    internal int TranscriptCount
    {
        get
        {
            lock (_stateGate)
            {
                return _transcript.Count;
            }
        }
    }

    internal bool IsCorrectionApplicable(CorrectionTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        lock (_stateGate)
        {
            return !_disposed
                && !_paused
                && _passwordState == PasswordState.No
                && _currentFocus is { } focus
                && focus.WindowHandle == transaction.TargetWindowHandle
                && transaction.SpanEntries.Count > 0
                && transaction.SpanEntries.All(entry => entry.EpochId == _epochId);
        }
    }

    private void HandleFocusChanged(FocusContext context)
    {
        lock (_stateGate)
        {
            if (_disposed)
            {
                return;
            }

            _currentFocus = context;
            _passwordState = context.PasswordState;
            InvalidateEpoch();
        }

        // A focus change invalidates any armed transaction, so the armed state must not survive it
        // and consume a key in the newly-focused control.
        _keystrokes.Disarm();
    }

    private void HandleKeyObserved(KeyEvent key)
    {
        lock (_stateGate)
        {
            if (_disposed || _paused)
            {
                return;
            }

            if (_passwordState != PasswordState.No || key.IsSelfInjected)
            {
                WipeTranscript();
                return;
            }

            // The key carries the top-level window that had focus when it was typed. Keystroke
            // capture and focus changes arrive on separate OS callback streams, so a key queued
            // while another window had focus can arrive after the focus context has moved on.
            // Gating on the carried origin rather than on arrival order is what keeps that key
            // from being retained against the wrong context (FR-003).
            //
            // This compares top-level to top-level. The focus stream normalizes with GA_ROOT
            // precisely so this comparison is possible: the raw WinEvent handle names the focused
            // control, which the keystroke stream cannot see, and comparing against it would
            // reject every keystroke in any application built from child controls.
            if (_currentFocus is not { } focus
                || (key.SourceWindowHandle != 0 && key.SourceWindowHandle != focus.WindowHandle))
            {
                WipeTranscript();
                return;
            }

            // Within one top-level window, focus can move between an ordinary field and a password
            // field without changing the correlation identity, and a key can be pressed in the new
            // control before the focus stream has published anything about it. The key carries the
            // control that had focus at the instant it was typed, so a key from a control other
            // than the one whose password state was resolved is discarded rather than attributed
            // to the previous control's state (FR-003, User Story 5).
            if (key.SourceControlHandle != 0
                && focus.ControlHandle is { } resolvedControl
                && key.SourceControlHandle != resolvedControl)
            {
                WipeTranscript();
                return;
            }

            var result = _wordAssembly.Append(key);
            if (result.Outcome != WordAssemblyOutcome.WordCompleted)
            {
                return;
            }

            var entry = new TranscriptEntry(
                Guid.NewGuid(),
                result.Text,
                result.ScanCodes,
                key.LayoutId,
                0,
                TranscriptEntryState.Complete,
                null,
                _epochId);
            _transcript.Enqueue(entry);
            while (_transcript.Count > MaxTranscriptEntries)
            {
                _transcript.Dequeue();
            }
        }
    }

    private void WipeTranscript()
    {
        _transcript.Clear();
        _wordAssembly.Reset();
    }

    /// <summary>
    /// Erases the transcript and advances the epoch, so no correction built before this point can
    /// still be applied.
    /// </summary>
    /// <remarks>
    /// Wiping alone is not sufficient at a lifecycle boundary. A transaction captured before the
    /// boundary holds its own span entries, so it survives the wipe; if the epoch does not move,
    /// that transaction matches again the moment the boundary is lifted and edits text the user
    /// asked to be discarded. Every invalidating event advances the epoch, not only focus changes.
    /// </remarks>
    private void InvalidateEpoch()
    {
        _epochId++;
        WipeTranscript();
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, nameof(CorrectionManager));

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_stateGate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            WipeTranscript();
            _paused = true;
        }

        _keystrokes.KeyObserved -= HandleKeyObserved;
        _keystrokes.SequenceGapDetected -= HandleSequenceGap;
        _focus.FocusChanged -= HandleFocusChanged;
        _keystrokes.Disarm();
        _ = _keystrokes.UninstallAsync();
        GC.SuppressFinalize(this);
    }
}
