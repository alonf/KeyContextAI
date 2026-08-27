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
        focus.Publish(new FocusContext((nint)1, 1, 1, null, null, null, null, null, true, true,
            PasswordState.No, null));

        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());
        Assert.Equal(1, manager.TranscriptCount);

        focus.Publish(new FocusContext((nint)2, 2, 2, null, null, null, null, null, true, true,
            PasswordState.No, null));

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void Pause_WipesTranscriptAndSuspendsCapture()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        keystrokes.Publish(Character('a'));
        keystrokes.Publish(Separator());
        manager.Pause();

        Assert.True(manager.IsSuspended);
        Assert.Equal(0, manager.TranscriptCount);
    }

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
            FocusChanged?.Invoke(context);
        }
    }
}
