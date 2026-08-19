namespace KeyContextAI.Core.Model;

/// <summary>What the detection engine decided about a run of text.</summary>
public enum CorrectionOutcome
{
    /// <summary>Leave the text alone. Always a valid answer, and the answer whenever uncertain.</summary>
    Ignore,

    /// <summary>A wrong layout was detected, but the user asked to be told rather than corrected.</summary>
    Notify,

    /// <summary>Replace the text and switch the layout.</summary>
    Correct,
}

/// <summary>Which tier produced a verdict. Drives the distinct feedback sound in FR-023.</summary>
public enum DetectionTier
{
    /// <summary>The local dictionary tier.</summary>
    Dictionary,

    /// <summary>The context-aware AI tier. Not present in iteration 001.</summary>
    Ai,
}

/// <summary>
/// The detection engine's decision about a span of typed text.
/// </summary>
/// <param name="Outcome">Correct, notify, or leave alone.</param>
/// <param name="TextAsTyped">What the user produced.</param>
/// <param name="TextIntended">The corrected form. Empty when <paramref name="Outcome"/> is
/// <see cref="CorrectionOutcome.Ignore"/>.</param>
/// <param name="TargetLayout">The layout the text was intended for. Null when ignoring.</param>
/// <param name="Confidence">0.0 to 1.0, compared against the caution level's threshold (FR-006).</param>
/// <param name="Tier">Which tier decided.</param>
/// <param name="TransactionId">Correlates verdict, injection and feedback, and lets a superseded
/// result be discarded rather than applied (FR-018).</param>
public sealed record CorrectionVerdict(
    CorrectionOutcome Outcome,
    string TextAsTyped,
    string TextIntended,
    LayoutId? TargetLayout,
    double Confidence,
    DetectionTier Tier,
    Guid TransactionId)
{
    /// <summary>A verdict to leave the text exactly as the user typed it.</summary>
    /// <param name="textAsTyped">The text being left alone.</param>
    /// <param name="tier">The tier that declined to correct.</param>
    public static CorrectionVerdict Leave(string textAsTyped, DetectionTier tier = DetectionTier.Dictionary) =>
        new(CorrectionOutcome.Ignore, textAsTyped, string.Empty, null, 0.0, tier, Guid.Empty);
}
