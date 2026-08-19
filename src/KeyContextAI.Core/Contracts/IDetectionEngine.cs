using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Decides whether a run of text was typed on the wrong layout, and if so what it was meant to be.
/// </summary>
/// <remarks>
/// A pure engine. Dictionary data is passed in rather than fetched, because engines make no accessor
/// calls. <see cref="CorrectionOutcome.Ignore"/> is always a valid answer and is the answer whenever
/// two candidates are comparably plausible — a false correction is worse than a missed one.
/// </remarks>
public interface IDetectionEngine
{
    /// <summary>
    /// Scores every candidate against the supplied dictionaries and returns a verdict.
    /// </summary>
    /// <param name="candidates">The interpretations to compare, including the as-typed text.</param>
    /// <param name="dictionaries">One snapshot per language under consideration.</param>
    /// <param name="caution">The user's caution level, which sets the confidence bar.</param>
    /// <returns>
    /// A verdict. Never throws: an empty candidate set, absent dictionaries, or an unresolvable
    /// ambiguity all produce <see cref="CorrectionOutcome.Ignore"/>.
    /// </returns>
    CorrectionVerdict Evaluate(
        IReadOnlyList<Candidate> candidates,
        IReadOnlyList<DictionarySnapshot> dictionaries,
        CautionLevel caution);
}
