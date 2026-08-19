using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Covers FR-006 specifically: the caution level must change what actually happens, not merely be
/// stored. These are the tests that would fail if the levels became decorative.
/// </summary>
public sealed class CautionLevelTests
{
    private static readonly LayoutId Greek = new("el-GR");

    /// <summary>
    /// A field resolved only by a decisive frequency gap — plausible, but a frequency argument
    /// rather than an unambiguous one.
    /// </summary>
    private static (Candidate[] Candidates, DictionarySnapshot[] Dictionaries) FrequencyResolvedCase()
    {
        Candidate[] candidates =
        [
            new(LayoutMaps.EnUs, "akuo", IsComplete: true, IsAsTyped: true),
            new(LayoutMaps.HeIl, "שלום", IsComplete: true, IsAsTyped: false),
            new(Greek, "ακυο", IsComplete: true, IsAsTyped: false),
        ];

        DictionarySnapshot[] dictionaries =
        [
            new(LayoutMaps.EnUs, ["hello"]),
            new(LayoutMaps.HeIl, ["שלום"], new Dictionary<string, int> { ["שלום"] = 10_000 }),
            new(Greek, ["ακυο"], new Dictionary<string, int> { ["ακυο"] = 1 }),
        ];

        return (candidates, dictionaries);
    }

    [Fact]
    public void Conservative_RefusesAFrequencyResolvedCorrection()
    {
        // A user who chose conservative does not want a frequency argument deciding what their text
        // says. This is the behavioural difference between the levels.
        var (candidates, dictionaries) = FrequencyResolvedCase();

        var verdict = new DetectionEngine().Evaluate(candidates, dictionaries, CautionLevel.Conservative);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Balanced_AcceptsAFrequencyResolvedCorrection()
    {
        var (candidates, dictionaries) = FrequencyResolvedCase();

        var verdict = new DetectionEngine().Evaluate(candidates, dictionaries, CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
        Assert.Equal(LayoutMaps.HeIl, verdict.TargetLayout);
    }

    [Fact]
    public void Conservative_StillCorrectsAnUnambiguousMatch()
    {
        // Conservative means cautious, not inert. An unambiguous match is still corrected.
        Candidate[] candidates =
        [
            new(LayoutMaps.EnUs, "akuo", IsComplete: true, IsAsTyped: true),
            new(LayoutMaps.HeIl, "שלום", IsComplete: true, IsAsTyped: false),
        ];
        DictionarySnapshot[] dictionaries =
        [
            new(LayoutMaps.EnUs, ["hello"]),
            new(LayoutMaps.HeIl, ["שלום"]),
        ];

        var verdict = new DetectionEngine().Evaluate(candidates, dictionaries, CautionLevel.Conservative);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
    }

    [Fact]
    public void EveryLevel_RefusesWhenNoCandidateIsRecognized()
    {
        // No level, however aggressive, invents a correction out of nothing.
        Candidate[] candidates =
        [
            new(LayoutMaps.EnUs, "xyzzy", IsComplete: true, IsAsTyped: true),
            new(LayoutMaps.HeIl, "ץחררט", IsComplete: true, IsAsTyped: false),
        ];
        DictionarySnapshot[] dictionaries =
        [
            new(LayoutMaps.EnUs, ["hello"]),
            new(LayoutMaps.HeIl, ["שלום"]),
        ];
        var engine = new DetectionEngine();

        foreach (var caution in Enum.GetValues<CautionLevel>())
        {
            Assert.Equal(
                CorrectionOutcome.Ignore,
                engine.Evaluate(candidates, dictionaries, caution).Outcome);
        }
    }

    [Fact]
    public void EveryLevel_RefusesWhenBothReadingsAreValidWords()
    {
        // Ambiguity is never resolved by turning the caution dial up.
        Candidate[] candidates =
        [
            new(LayoutMaps.EnUs, "so", IsComplete: true, IsAsTyped: true),
            new(LayoutMaps.HeIl, "בם", IsComplete: true, IsAsTyped: false),
        ];
        DictionarySnapshot[] dictionaries =
        [
            new(LayoutMaps.EnUs, ["so"]),
            new(LayoutMaps.HeIl, ["בם"]),
        ];
        var engine = new DetectionEngine();

        foreach (var caution in Enum.GetValues<CautionLevel>())
        {
            Assert.Equal(
                CorrectionOutcome.Ignore,
                engine.Evaluate(candidates, dictionaries, caution).Outcome);
        }
    }
}
