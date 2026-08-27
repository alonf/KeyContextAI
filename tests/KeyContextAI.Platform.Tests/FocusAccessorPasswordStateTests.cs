using System.Windows.Automation;
using KeyContextAI.Core.Model;
using KeyContextAI.Platform.System;

namespace KeyContextAI.Platform.Tests;

/// <summary>
/// A UI Automation provider that does not implement IsPassword yields the property's default
/// false through the plain accessor. That silence must classify as Unknown — fail-closed under
/// the hardening gate — never as explicitly safe.
/// </summary>
public sealed class FocusAccessorPasswordStateTests
{
    [Fact]
    public void UnsupportedProvider_FailsClosedAsUnknown()
    {
        Assert.Equal(
            PasswordState.Unknown,
            FocusAccessor.MapPasswordPropertyForTest(AutomationElement.NotSupported));
    }

    [Fact]
    public void MissingValue_FailsClosedAsUnknown()
    {
        Assert.Equal(PasswordState.Unknown, FocusAccessor.MapPasswordPropertyForTest(null));
    }

    [Fact]
    public void ExplicitTrue_IsYes()
    {
        Assert.Equal(PasswordState.Yes, FocusAccessor.MapPasswordPropertyForTest(true));
    }

    [Fact]
    public void ExplicitFalse_IsNo()
    {
        Assert.Equal(PasswordState.No, FocusAccessor.MapPasswordPropertyForTest(false));
    }
}
