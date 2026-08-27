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
        _focus.FocusChanged += HandleFocusChanged;
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
    public Task StartAsync() => _keystrokes.InstallAsync();

    /// <summary>Pauses observation and erases all in-memory typing data.</summary>
    public void Pause()
    {
        lock (_stateGate)
        {
            ThrowIfDisposed();
            _paused = true;
            WipeTranscript();
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
            WipeTranscript();
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

            _epochId++;
            _currentFocus = context;
            _passwordState = context.PasswordState;
            WipeTranscript();
        }
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
        _focus.FocusChanged -= HandleFocusChanged;
        _keystrokes.Disarm();
        _ = _keystrokes.UninstallAsync();
        GC.SuppressFinalize(this);
    }
}
