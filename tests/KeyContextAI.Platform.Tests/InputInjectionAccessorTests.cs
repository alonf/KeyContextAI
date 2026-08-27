using KeyContextAI.Core.Model;
using KeyContextAI.Platform.Input;

namespace KeyContextAI.Platform.Tests;

public sealed class InputInjectionAccessorTests
{
    [Fact]
    public void BuildCorrectionSteps_AppendsBackspacesTextAndSuppressedKeyWithSelfInjectionTag()
    {
        var tx = new CorrectionTransaction(
            Guid.NewGuid(),
            2,
            "ab",
            new LayoutId("he-IL"),
            new KeyEvent(28, 13, null, new LayoutId("en-US"), KeyEventKind.Committing, false, 0),
            new IntPtr(1234),
            []);

        var steps = InputInjectionAccessor.BuildCorrectionSteps(tx);

        Assert.Equal(10, steps.Count);
        Assert.All(steps, step => Assert.Equal(NativeInputTags.SelfInjectionTag, step.ExtraInfo));

        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyDown, steps[0].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyUp, steps[1].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyDown, steps[2].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyUp, steps[3].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.UnicodeDown, steps[4].Kind);
        Assert.Equal('a', steps[4].UnicodeChar);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.UnicodeUp, steps[5].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.UnicodeDown, steps[6].Kind);
        Assert.Equal('b', steps[6].UnicodeChar);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.UnicodeUp, steps[7].Kind);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyDown, steps[8].Kind);
        Assert.Equal(13, steps[8].VirtualKey);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyUp, steps[9].Kind);
    }

    [Fact]
    public void BuildReinjectSteps_UsesTheOriginalKeyAndTag()
    {
        var key = new KeyEvent(28, 13, null, new LayoutId("en-US"), KeyEventKind.Committing, false, 0);

        var steps = InputInjectionAccessor.BuildReinjectSteps(key);

        Assert.Equal(2, steps.Count);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyDown, steps[0].Kind);
        Assert.Equal(13, steps[0].VirtualKey);
        Assert.Equal(InputInjectionAccessor.InjectionStepKind.KeyUp, steps[1].Kind);
        Assert.All(steps, step => Assert.Equal(NativeInputTags.SelfInjectionTag, step.ExtraInfo));
    }
}
