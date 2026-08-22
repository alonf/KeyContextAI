using System.Drawing;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

public sealed class FocusContextTests
{
    [Fact]
    public void DefaultPasswordState_FailsClosedAsUnknown()
    {
        Assert.Equal(PasswordState.Unknown, default(PasswordState));
    }

    [Fact]
    public void FocusContext_PreservesWindowAndCaretMetadata()
    {
        var caret = new Point(42, 84);

        var context = new FocusContext(
            (nint)1234,
            5678,
            9012,
            "Editor",
            "Notepad",
            "textBox1",
            "ControlType.Edit",
            "Main text field",
            true,
            true,
            PasswordState.No,
            caret);

        Assert.Equal((nint)1234, context.WindowHandle);
        Assert.Equal(5678, context.ProcessId);
        Assert.Equal(9012, context.ThreadId);
        Assert.Equal("Editor", context.WindowTitle);
        Assert.Equal("Notepad", context.WindowClass);
        Assert.Equal("textBox1", context.AutomationId);
        Assert.Equal("ControlType.Edit", context.ControlType);
        Assert.Equal("Main text field", context.AutomationName);
        Assert.Equal(caret, context.CaretPosition);
    }
}
