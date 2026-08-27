using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Managers;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// The fail-closed guarantee must hold across the window in which a focus change has happened but
/// the password classification is not yet known.
/// </summary>
public sealed class CorrectionManagerFailClosedTests
{
    [Fact]
    public void ProvisionalUnknownFocus_SuspendsCaptureBeforeStateResolves()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        focus.Publish(Context(1, PasswordState.No));
        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());
        Assert.Equal(1, manager.TranscriptCount);

        // The accessor announces the new boundary as Unknown before its probe completes.
        focus.Publish(Context(2, PasswordState.Unknown));

        Assert.True(manager.IsSuspended);
        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void KeysTypedDuringUnresolvedFocus_AreNotRetained()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        focus.Publish(Context(1, PasswordState.No));
        focus.Publish(Context(2, PasswordState.Unknown));

        keystrokes.Publish(Character('s'));
        keystrokes.Publish(Character('e'));
        keystrokes.Publish(Separator());

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void UnknownResolvingToPassword_NeverRetainsText()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        focus.Publish(Context(1, PasswordState.No));
        focus.Publish(Context(2, PasswordState.Unknown));
        keystrokes.Publish(Character('p'));
        focus.Publish(Context(2, PasswordState.Yes));
        keystrokes.Publish(Character('w'));
        keystrokes.Publish(Separator());

        Assert.True(manager.IsSuspended);
        Assert.Equal(0, manager.TranscriptCount);
    }

    private static FocusContext Context(int handle, PasswordState state) =>
        new((nint)handle, handle + 1000, handle, handle, null, null, null, null, null, true, true,
            state, null);

    private static KeyEvent Character(char value) =>
        new(30, 65, value, new LayoutId("en-US"), KeyEventKind.Character, false, 0);

    private static KeyEvent Separator() =>
        new(57, 32, ' ', new LayoutId("en-US"), KeyEventKind.Separator, false, 0);

    private sealed class FakeKeystrokes : IKeystrokeAccessor
    {
        public event Action<KeyEvent>? KeyObserved;

        public event Action? SequenceGapDetected;


        public void PublishSequenceGap() => SequenceGapDetected?.Invoke();

        public void Publish(KeyEvent key) => KeyObserved?.Invoke(key);

        public void Arm(SuppressionToken token) { }

        public void Disarm() { }

        public Task InstallAsync() => Task.CompletedTask;

        public Task UninstallAsync() => Task.CompletedTask;
    }

    private sealed class FakeFocus : IFocusAccessor
    {
        private FocusContext? _current;

        public PasswordState State { get; set; }

        public event Action<FocusContext>? FocusChanged;

        public PasswordState IsPasswordContext() => State;

        public bool TryGetCaretPosition(out System.Drawing.Point p)
        {
            p = default;
            return false;
        }

        public void Publish(FocusContext context)
        {
            State = context.PasswordState;
            _current = context;
            FocusChanged?.Invoke(context);
        }

        public void PublishCurrentFocus()
        {
            if (_current is { } current)
            {
                FocusChanged?.Invoke(current);
            }
        }
    }
}
