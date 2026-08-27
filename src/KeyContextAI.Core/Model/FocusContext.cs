using System.Drawing;

namespace KeyContextAI.Core.Model;

/// <summary>
/// A snapshot of the foreground window and its focused control.
/// </summary>
/// <param name="WindowHandle">The <em>top-level</em> window handle, normalized with GA_ROOT. This is
/// the correlation identity: it is the only identity both the focus stream and the keystroke stream
/// can derive independently, so it is what the two streams are matched on.</param>
/// <param name="ControlHandle">The <em>focused control</em> handle, which is frequently a child of
/// <paramref name="WindowHandle"/>. This is the password-gate identity: password state is a property
/// of the control, not of the window that hosts it. Null when the platform did not report one.</param>
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
/// <remarks>
/// <para>
/// Two distinct notions of window identity are carried deliberately. Conflating them is a defect in
/// both directions: matching the streams on control identity discards every ordinary keystroke,
/// because the keystroke stream can only see the top-level window; gating password state on
/// top-level identity misses a password control that shares its window with ordinary fields.
/// </para>
/// <para>
/// Residual limit: where an application draws several logical fields inside one control — so the
/// distinction exists only in UI Automation and not in any window handle — neither identity can
/// separate them. That case is handled by failing closed on <see cref="PasswordState.Unknown"/>
/// rather than by a handle comparison, because no handle exists to compare.
/// </para>
/// </remarks>
public sealed record FocusContext(
    nint WindowHandle,
    nint? ControlHandle,
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
