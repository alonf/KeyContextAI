using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Managers;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

public sealed class CorrectionManagerFocusTests
{
    [Fact]
    public void CorrectionFromPreviousWindow_IsNotApplicableAfterFocusChanges()
    {
        var keystrokes = new FakeKeystrokes();
        var focus = new FakeFocus();
        using var manager = new CorrectionManager(keystrokes, focus, new WordAssemblyEngine());
        focus.Publish(Context((nint)1, 1));

        var entry = new TranscriptEntry(
            Guid.NewGuid(), "akuo", [30], new LayoutId("en-US"), 0,
            TranscriptEntryState.Complete, null, 1);
        var transaction = new CorrectionTransaction(
            Guid.NewGuid(), 4, "שלום", new LayoutId("he-IL"), null, (nint)1, [entry]);

        focus.Publish(Context((nint)2, 2));

        Assert.False(manager.IsCorrectionApplicable(transaction));
    }

    private static FocusContext Context(nint window, int process) =>
        new(window, window + 1000, process, process, null, null, null, null, null, true, true,
            PasswordState.No, null);

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

        public event Action<FocusContext>? FocusChanged;
        public PasswordState IsPasswordContext() => PasswordState.No;
        public bool TryGetCaretPosition(out System.Drawing.Point p)
        {
            p = default;
            return false;
        }

        public void Publish(FocusContext context)
        {
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
