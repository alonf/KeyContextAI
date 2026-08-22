using System.Drawing;

namespace KeyContextAI.Core.Model;

/// <summary>
/// A snapshot of the foreground window and its focused control.
/// </summary>
/// <param name="WindowHandle">The foreground window handle.</param>
/// <param name="ProcessId">The process that owns the foreground window.</param>
/// <param name="ThreadId">The thread that owns the foreground window.</param>
/// <param name="WindowTitle">The window title, when available.</param>
/// <param name="WindowClass">The window class name, when available.</param>
/// <param name="AutomationId">The UI Automation identifier of the focused element, when available.</param>
/// <param name="ControlType">The UI Automation control type, when available.</param>
/// <param name="AutomationName">The UI Automation name of the focused element, when available.</param>
/// <param name="IsForeground">True when the context is the active foreground window.</param>
/// <param name="IsFocusedControl">True when the focused control changed, not just the foreground window.</param>
/// <param name="PasswordState">The password classification of the focused control.</param>
/// <param name="CaretPosition">The caret position in screen coordinates, when available.</param>
public sealed record FocusContext(
    nint WindowHandle,
    int ProcessId,
    int ThreadId,
    string? WindowTitle,
    string? WindowClass,
    string? AutomationId,
    string? ControlType,
    string? AutomationName,
    bool IsForeground,
    bool IsFocusedControl,
    PasswordState PasswordState,
    Point? CaretPosition);
