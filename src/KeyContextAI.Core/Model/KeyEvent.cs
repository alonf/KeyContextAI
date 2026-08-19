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
    long TimestampTicks);
