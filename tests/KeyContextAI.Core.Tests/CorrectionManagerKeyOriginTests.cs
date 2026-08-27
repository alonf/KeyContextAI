using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Managers;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Keystroke capture and focus changes arrive on separate OS callback streams with no shared
/// sequence, so a keystroke must be gated on the origin it carries rather than on the focus
/// context that happens to be current when it is dequeued.
/// </summary>
public sealed class CorrectionManagerKeyOriginTests
{
    [Fact]
    public void KeyFromAnotherWindow_IsNotRetainedAgainstTheCurrentContext()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        // Typed while window 2 had focus — a password field, say — but consumed after focus has
        // already resolved to the ordinary window 1.
        keystrokes.Publish(Character('s', sourceWindow: 2));
        keystrokes.Publish(Character('e', sourceWindow: 2));
        keystrokes.Publish(Separator(sourceWindow: 2));

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void KeyFromTheCurrentWindow_IsRetained()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        keystrokes.Publish(Character('a', sourceWindow: 1));
        keystrokes.Publish(Separator(sourceWindow: 1));

        Assert.Equal(1, manager.TranscriptCount);
    }

    [Fact]
    public void KeyArrivingBeforeAnyFocusContext_IsNotRetained()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());

        keystrokes.Publish(Character('a', sourceWindow: 1));
        keystrokes.Publish(Separator(sourceWindow: 1));

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void SequenceGap_InvalidatesTheEpochAndWipesTheTranscript()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        keystrokes.Publish(Character('a', sourceWindow: 1));
        keystrokes.Publish(Separator(sourceWindow: 1));
        Assert.Equal(1, manager.TranscriptCount);

        // Capture overflowed: what remains no longer describes the text on screen.
        keystrokes.PublishSequenceGap();

        Assert.Equal(0, manager.TranscriptCount);
    }

    [Fact]
    public void SequenceGap_MakesAnEarlierTransactionInapplicable()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus { State = PasswordState.No };
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context(1, PasswordState.No));

        keystrokes.Publish(Character('a', sourceWindow: 1));
        keystrokes.Publish(Separator(sourceWindow: 1));

        var stale = new CorrectionTransaction(
            Guid.NewGuid(),
            1,
            "b",
            new LayoutId("he-IL"),
            null,
            (nint)1,
            [new TranscriptEntry(Guid.NewGuid(), "a", [], new LayoutId("en-US"), 0,
                TranscriptEntryState.Complete, null, 1)]);

        keystrokes.PublishSequenceGap();

        Assert.False(manager.IsCorrectionApplicable(stale));
    }

    private static FocusContext Context(int handle, PasswordState state) =>
        new((nint)handle, handle, handle, null, null, null, null, null, true, true, state, null);

    private static KeyEvent Character(char value, int sourceWindow) =>
        new(30, 65, value, new LayoutId("en-US"), KeyEventKind.Character, false, 0, (nint)sourceWindow);

    private static KeyEvent Separator(int sourceWindow) =>
        new(57, 32, ' ', new LayoutId("en-US"), KeyEventKind.Separator, false, 0, (nint)sourceWindow);

    private sealed class FakeKeystrokes : IKeystrokeAccessor
    {
        public event Action<KeyEvent>? KeyObserved;

        public event Action? SequenceGapDetected;

        public void Publish(KeyEvent key) => KeyObserved?.Invoke(key);

        public void PublishSequenceGap() => SequenceGapDetected?.Invoke();

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
