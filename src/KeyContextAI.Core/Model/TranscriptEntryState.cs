namespace KeyContextAI.Core.Model;

/// <summary>The lifecycle state of a transcript entry.</summary>
public enum TranscriptEntryState
{
    /// <summary>The entry is still being assembled.</summary>
    InProgress,

    /// <summary>The entry is complete and ready for evaluation.</summary>
    Complete,

    /// <summary>The entry has been evaluated but not yet committed.</summary>
    VerdictReady,

    /// <summary>The entry was corrected.</summary>
    Corrected,

    /// <summary>The entry was rejected or abandoned.</summary>
    Rejected,
}
