namespace KeyContextAI.Core.Model;

/// <summary>
/// One word, or word in progress, in the rolling typing journal.
/// </summary>
public sealed record TranscriptEntry(
    Guid Id,
    string Text,
    IReadOnlyList<int> ScanCodes,
    LayoutId TypedInLayout,
    int StartOffset,
    TranscriptEntryState State,
    CorrectionVerdict? Verdict,
    int EpochId);
