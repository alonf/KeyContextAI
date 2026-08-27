namespace KeyContextAI.Core.Model;

/// <summary>
/// One observed keystroke, as published by the keystroke accessor.
/// </summary>
/// <param name="ScanCode">The hardware scan code, which is layout-independent and is what layout
/// translation works from.</param>
/// <param name="VirtualKey">The Windows virtual-key code.</param>
/// <param name="Character">The character the active layout produced, when it produced one.</param>
/// <param name="LayoutId">The layout that was active when the key was pressed.</param>
/// <param name="Kind">What this key means to word assembly.</param>
/// <param name="IsSelfInjected">True when this tool injected the key. Self-injected events never
/// re-enter the pipeline (FR-013).</param>
/// <param name="TimestampTicks">A monotonic timestamp used for ordering and typing-speed
/// measurement.</param>
/// <param name="SourceWindowHandle">The foreground window that had focus when the key was pressed,
/// captured at the moment of capture rather than sampled later. Keystroke capture and focus changes
/// arrive on separate OS callback streams with no shared sequence, so a key must carry its own
/// origin or it can be evaluated against a focus context that was not current when it was typed
/// (FR-003).</param>
/// <param name="SuppressedToken">Set when this key was suppressed by an armed transaction, so the
/// correction path can correlate the suppression with the transaction that armed it.</param>
/// <remarks>
/// Transient by requirement: a key event exists only in memory and is never written to disk or
/// transmitted (FR-004).
/// </remarks>
public sealed record KeyEvent(
    int ScanCode,
    int VirtualKey,
    char? Character,
    LayoutId LayoutId,
    KeyEventKind Kind,
    bool IsSelfInjected,
    long TimestampTicks,
    nint SourceWindowHandle = 0,
    SuppressionToken? SuppressedToken = null);
