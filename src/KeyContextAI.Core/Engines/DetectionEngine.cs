using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Engines;

/// <inheritdoc cref="IDetectionEngine" />
/// <remarks>
/// The algorithm is deliberately unclever. Each candidate is either recognized by its language's
/// dictionary or it is not; a correction is offered only when exactly one candidate other than the
/// as-typed text is recognized, the as-typed text is not, and no second candidate is comparably
/// plausible. Everything else returns <see cref="CorrectionOutcome.Ignore"/>.
///
/// That conservatism is the point rather than a limitation: SC-001 allows fewer than one false
/// correction per thousand, and a scoring scheme that resolves ambiguity by preference would spend
/// that budget on exactly the cases a human would not want touched.
/// </remarks>
public sealed class DetectionEngine : IDetectionEngine
{
    /// <summary>
    /// How much more frequent a candidate must be than its nearest rival before frequency is
    /// allowed to break a tie between two recognized words.
    /// </summary>
    private const int DecisiveFrequencyRatio = 100;

    /// <inheritdoc />
    public CorrectionVerdict Evaluate(
        IReadOnlyList<Candidate> candidates,
        IReadOnlyList<DictionarySnapshot> dictionaries,
        CautionLevel caution)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(dictionaries);

        var asTyped = candidates.FirstOrDefault(c => c.IsAsTyped);
        if (asTyped is null || candidates.Count == 0 || dictionaries.Count == 0)
        {
            return CorrectionVerdict.Leave(asTyped?.Text ?? string.Empty);
        }

        // The user has affirmed this word, so it is correct by their own instruction regardless of
        // what any dictionary says (FR-009a).
        if (DictionaryFor(dictionaries, asTyped.Layout)?.IsNeverCorrect(asTyped.Text) == true)
        {
            return CorrectionVerdict.Leave(asTyped.Text);
        }

        // Text the user's own layout already recognizes is text they meant to type.
        if (IsRecognized(asTyped, dictionaries))
        {
            return CorrectionVerdict.Leave(asTyped.Text);
        }

        var recognized = candidates
            .Where(c => !c.IsAsTyped && c.IsComplete && IsRecognized(c, dictionaries))
            .ToList();

        if (recognized.Count == 0)
        {
            // Gibberish in every layout: a proper noun, a typo, an identifier. Not ours to touch.
            return CorrectionVerdict.Leave(asTyped.Text);
        }

        var winner = ResolveWinner(recognized, dictionaries);
        if (winner is null)
        {
            // Two or more plausible readings. Choosing between them would be a guess.
            return CorrectionVerdict.Leave(asTyped.Text);
        }

        var confidence = ConfidenceFor(winner, recognized);
        if (confidence < ThresholdFor(caution))
        {
            return CorrectionVerdict.Leave(asTyped.Text);
        }

        return new CorrectionVerdict(
            CorrectionOutcome.Correct,
            asTyped.Text,
            winner.Text,
            winner.Layout,
            confidence,
            DetectionTier.Dictionary,
            Guid.NewGuid());
    }

    /// <summary>
    /// Picks the single winner among recognized candidates, or null when the field is ambiguous.
    /// Frequency breaks a tie only when one candidate is overwhelmingly more common; a narrow
    /// frequency edge is not evidence, it is noise.
    /// </summary>
    private static Candidate? ResolveWinner(
        List<Candidate> recognized,
        IReadOnlyList<DictionarySnapshot> dictionaries)
    {
        if (recognized.Count == 1)
        {
            return recognized[0];
        }

        var ranked = recognized
            .Select(c => (Candidate: c, Frequency: DictionaryFor(dictionaries, c.Layout)?.FrequencyOf(c.Text) ?? 0))
            .OrderByDescending(x => x.Frequency)
            .ToList();

        var best = ranked[0];
        var runnerUp = ranked[1];

        var decisive = best.Frequency > 0
            && best.Frequency >= runnerUp.Frequency * DecisiveFrequencyRatio;

        return decisive ? best.Candidate : null;
    }

    /// <summary>
    /// Confidence in the chosen candidate.
    /// </summary>
    /// <remarks>
    /// A lone recognized reading of otherwise-gibberish text is strong evidence. A field that needed
    /// a frequency tie-break is weaker — but not weak, because <see cref="ResolveWinner"/> has
    /// already rejected every field where the gap was not decisive, so anything scored here won by a
    /// wide margin. It is scored below the conservative threshold deliberately: a user who asked for
    /// conservative behavior is exactly the user who does not want a frequency argument deciding
    /// what their text should say.
    /// </remarks>
    private static double ConfidenceFor(Candidate winner, List<Candidate> recognized)
    {
        if (recognized.Count == 1)
        {
            // Longer runs are less likely to be coincidence than two-letter ones.
            return winner.Text.Length >= 3 ? 0.97 : 0.85;
        }

        return 0.85;
    }

    /// <summary>The confidence a correction must reach before it is applied (FR-006).</summary>
    private static double ThresholdFor(CautionLevel caution) => caution switch
    {
        CautionLevel.Conservative => 0.95,
        CautionLevel.Balanced => 0.80,
        CautionLevel.Aggressive => 0.60,
        _ => 0.95,
    };

    private static bool IsRecognized(Candidate candidate, IReadOnlyList<DictionarySnapshot> dictionaries) =>
        DictionaryFor(dictionaries, candidate.Layout) is { } dictionary
        && dictionary.Contains(candidate.Text);

    private static DictionarySnapshot? DictionaryFor(
        IReadOnlyList<DictionarySnapshot> dictionaries,
        LayoutId layout)
    {
        foreach (var dictionary in dictionaries)
        {
            if (dictionary.Language == layout)
            {
                return dictionary;
            }
        }

        return null;
    }
}
