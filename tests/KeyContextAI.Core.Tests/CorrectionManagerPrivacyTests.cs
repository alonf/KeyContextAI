using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Managers;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

public sealed class CorrectionManagerPrivacyTests
{
    [Fact]
    public void UnknownPasswordState_DoesNotRetainObservedText()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.Unknown };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());

        Assert.True(manager.IsSuspended);
        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void FocusChange_WipesTranscriptAndResetsAssembly()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());
        Assert.Equal(1, manager.TranscriptCount);

        focus.Publish(Context(2, PasswordState.No));

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void Pause_WipesTranscriptAndSuspendsCapture()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        // The transcript must hold text before the pause, or the wipe assertion below passes
        // vacuously and the pause-boundary privacy control goes unexercised.
        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());
        Assert.Equal(1, manager.TranscriptCount);

        manager.Pause();

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
