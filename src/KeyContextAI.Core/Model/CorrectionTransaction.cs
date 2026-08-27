namespace KeyContextAI.Core.Model;

/// <summary>
/// The atomic unit of text replacement, including the suppressed key when Option B is armed.
/// </summary>
public sealed record CorrectionTransaction(
    Guid TransactionId,
    int BackspaceCount,
    string ReplacementText,
    LayoutId TargetLayout,
    KeyEvent? SuppressedKey,
    IntPtr TargetWindowHandle,
    IReadOnlyList<TranscriptEntry> SpanEntries);
