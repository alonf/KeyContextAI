using KeyContextAI.Core.Model;
using KeyContextAI.Platform.System;

namespace KeyContextAI.Platform.Tests;

public sealed class LayoutAccessorTests
{
    [Fact]
    public void LayoutIdFromKeyboardLayout_UsesLowWordLanguageId()
    {
        var layout = LayoutAccessor.LayoutIdFromKeyboardLayout((nint)0x0000_0409);

        Assert.Equal(new LayoutId("en-US"), layout);
    }

    [Fact]
    public void LayoutIdFromKeyboardLayout_UsesUndForUnknownLanguageId()
    {
        var layout = LayoutAccessor.LayoutIdFromKeyboardLayout((nint)0x0000_FFFF);

        Assert.Equal(new LayoutId("und"), layout);
    }
}
