namespace KeyContextAI.Core.Model;

/// <summary>
/// How willing the user is to have text corrected. Sets both the dictionary confidence bar and
/// whether an ambiguous case escalates to the AI tier (FR-006).
/// </summary>
public enum CautionLevel
{
    /// <summary>Correct only unambiguous dictionary matches; never escalate to the AI tier.</summary>
    Conservative,

    /// <summary>Correct clear matches and escalate ambiguous ones to the AI tier.</summary>
    Balanced,

    /// <summary>Escalate more readily and accept a lower AI confidence.</summary>
    Aggressive,
}
