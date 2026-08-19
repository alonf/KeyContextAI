using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Covers FR-005a (target-layout resolution with two versus more layouts), FR-006 (caution levels
/// set the confidence bar), FR-009 (never re-correct an affirmed word), and SC-012.
/// </summary>
/// <remarks>
/// The governing property throughout: when the engine is unsure it returns
/// <see cref="CorrectionOutcome.Ignore"/>. A false correction is worse than a missed one.
/// </remarks>
public sealed class DetectionEngineTests
{
    private static DictionarySnapshot English(params string[] words) =>
        new(LayoutMaps.EnUs, words);

    private static DictionarySnapshot Hebrew(params string[] words) =>
        new(LayoutMaps.HeIl, words);

    private static Candidate AsTyped(LayoutId layout, string text) =>
        new(layout, text, IsComplete: true, IsAsTyped: true);

    private static Candidate Alternative(LayoutId layout, string text) =>
        new(layout, text, IsComplete: true, IsAsTyped: false);

    private static IDetectionEngine NewEngine() => new DetectionEngine();

    [Fact]
    public void Evaluate_GibberishAsTypedButValidWhenRemapped_Corrects()
    {
        // The canonical case: "akuo" is not an English word, "שלום" is a Hebrew one.
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [English("hello", "world"), Hebrew("שלום", "תודה")],
            CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
        Assert.Equal("שלום", verdict.TextIntended);
        Assert.Equal(LayoutMaps.HeIl, verdict.TargetLayout);
        Assert.Equal(DetectionTier.Dictionary, verdict.Tier);
    }

    [Fact]
    public void Evaluate_ValidAsTyped_LeavesItAlone()
    {
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "hello"), Alternative(LayoutMaps.HeIl, "יקךךם")],
            [English("hello"), Hebrew("שלום")],
            CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_ValidInBothLayouts_LeavesItAloneRatherThanGuessing()
    {
        // The word means something in both. Correcting would be a coin flip, and a wrong flip
        // mangles text the user meant.
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "so"), Alternative(LayoutMaps.HeIl, "בם")],
            [English("so"), Hebrew("בם")],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_NothingRecognizedAnywhere_LeavesItAlone()
    {
        // A proper noun, a password fragment, a typo. Not our business.
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "xyzzy"), Alternative(LayoutMaps.HeIl, "ץחררט")],
            [English("hello"), Hebrew("שלום")],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_IncompleteCandidate_IsNeverChosen()
    {
        // A candidate with an unmappable key cannot be typed back accurately.
        var incomplete = new Candidate(LayoutMaps.HeIl, "של�ם", IsComplete: false, IsAsTyped: false);

        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), incomplete],
            [English("hello"), Hebrew("של�ם")],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_AffirmedWord_IsNeverCorrected()
    {
        // FR-009 / FR-009a: the user already told us this word is fine as typed.
        var english = new DictionarySnapshot(LayoutMaps.EnUs, ["hello"], neverCorrect: ["akuo"]);

        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [english, Hebrew("שלום")],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_NotifyOnlyIsNotDecidedHere()
    {
        // The engine reports what it found; whether to act or merely notify is the manager's flow
        // decision driven by the user's mode. The engine never returns Notify on its own.
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [English("hello"), Hebrew("שלום")],
            CautionLevel.Balanced);

        Assert.NotEqual(CorrectionOutcome.Notify, verdict.Outcome);
    }

    [Theory]
    [InlineData(CautionLevel.Conservative)]
    [InlineData(CautionLevel.Balanced)]
    [InlineData(CautionLevel.Aggressive)]
    public void Evaluate_AnUnambiguousMatch_CorrectsAtEveryCautionLevel(CautionLevel caution)
    {
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [English("hello"), Hebrew("שלום")],
            caution);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_ThreeLayouts_OneClearWinner_ResolvesTheTarget()
    {
        // FR-005a / SC-012: with more than two layouts the target is worked out by comparing every
        // candidate, not by assuming "the other one".
        var greek = new LayoutId("el-GR");

        var verdict = NewEngine().Evaluate(
            [
                AsTyped(LayoutMaps.EnUs, "akuo"),
                Alternative(LayoutMaps.HeIl, "שלום"),
                Alternative(greek, "ακυο"),
            ],
            [English("hello"), Hebrew("שלום"), new DictionarySnapshot(greek, ["γεια"])],
            CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
        Assert.Equal(LayoutMaps.HeIl, verdict.TargetLayout);
    }

    [Fact]
    public void Evaluate_ThreeLayouts_TwoPlausibleWinners_LeavesItAlone()
    {
        // SC-012's other half: ambiguity between candidates means no correction at all.
        var greek = new LayoutId("el-GR");

        var verdict = NewEngine().Evaluate(
            [
                AsTyped(LayoutMaps.EnUs, "akuo"),
                Alternative(LayoutMaps.HeIl, "שלום"),
                Alternative(greek, "ακυο"),
            ],
            [English("hello"), Hebrew("שלום"), new DictionarySnapshot(greek, ["ακυο"])],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_FrequencyBreaksATieOnlyWhenItIsDecisive()
    {
        // Two recognized candidates, but one is far more common. Frequency may break the tie; it
        // must not manufacture confidence where the words are comparably common.
        var greek = new LayoutId("el-GR");
        var hebrewCommon = new DictionarySnapshot(
            LayoutMaps.HeIl, ["שלום"], new Dictionary<string, int> { ["שלום"] = 10_000 });
        var greekRare = new DictionarySnapshot(
            greek, ["ακυο"], new Dictionary<string, int> { ["ακυο"] = 1 });

        var verdict = NewEngine().Evaluate(
            [
                AsTyped(LayoutMaps.EnUs, "akuo"),
                Alternative(LayoutMaps.HeIl, "שלום"),
                Alternative(greek, "ακυο"),
            ],
            [English("hello"), hebrewCommon, greekRare],
            CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Correct, verdict.Outcome);
        Assert.Equal(LayoutMaps.HeIl, verdict.TargetLayout);
    }

    [Fact]
    public void Evaluate_NoCandidates_LeavesItAlone()
    {
        var verdict = NewEngine().Evaluate([], [English("hello")], CautionLevel.Balanced);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_NoDictionaries_LeavesItAlone()
    {
        // A missing or refused dictionary pack must degrade to doing nothing, never to guessing.
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_CandidateWithNoMatchingDictionary_IsNotChosen()
    {
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [English("hello")],
            CautionLevel.Aggressive);

        Assert.Equal(CorrectionOutcome.Ignore, verdict.Outcome);
    }

    [Fact]
    public void Evaluate_AssignsAFreshTransactionIdPerCorrection()
    {
        // The id is what lets a superseded correction be discarded rather than applied.
        var engine = NewEngine();
        Candidate[] candidates = [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")];
        DictionarySnapshot[] dictionaries = [English("hello"), Hebrew("שלום")];

        var first = engine.Evaluate(candidates, dictionaries, CautionLevel.Balanced);
        var second = engine.Evaluate(candidates, dictionaries, CautionLevel.Balanced);

        Assert.NotEqual(Guid.Empty, first.TransactionId);
        Assert.NotEqual(first.TransactionId, second.TransactionId);
    }

    [Fact]
    public void Evaluate_ConfidenceIsWithinRange()
    {
        var verdict = NewEngine().Evaluate(
            [AsTyped(LayoutMaps.EnUs, "akuo"), Alternative(LayoutMaps.HeIl, "שלום")],
            [English("hello"), Hebrew("שלום")],
            CautionLevel.Balanced);

        Assert.InRange(verdict.Confidence, 0.0, 1.0);
    }
}
