using System.Text.Json;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;
using KeyContextAI.Platform.Storage;

namespace KeyContextAI.Platform.Tests;

/// <summary>
/// Measures the dictionary tier against the golden corpus and produces the number SC-001 constrains:
/// how often the engine changes text that was already correct.
/// </summary>
/// <remarks>
/// This is the deliverable of iteration 001. It runs against the real shipped data packs through the
/// real accessor, not against hand-built fixtures, because a measurement taken on data assembled to
/// match the detector would not be a measurement at all.
/// </remarks>
public sealed class CorpusAccuracyTests
{
    private const string PairId = "en-US<->he-IL";
    private static readonly LayoutId EnUs = new("en-US");
    private static readonly LayoutId HeIl = new("he-IL");

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "KeyContextAI.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }

    private sealed record CorpusCase(
        string Id,
        string Kind,
        string TypedLayout,
        string OnScreen,
        string? Intended,
        string? IntendedLayout,
        bool KnownCoverageGap);

    private static IReadOnlyList<CorpusCase> LoadCorpus()
    {
        var path = Path.Combine(RepositoryRoot(), "tests", "corpus", "en-he-corpus.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return [.. document.RootElement.GetProperty("cases").EnumerateArray().Select(c => new CorpusCase(
            c.GetProperty("id").GetString()!,
            c.GetProperty("kind").GetString()!,
            c.GetProperty("typed_layout").GetString()!,
            c.GetProperty("on_screen").GetString()!,
            c.TryGetProperty("intended", out var i) ? i.GetString() : null,
            c.TryGetProperty("intended_layout", out var il) ? il.GetString() : null,
            c.TryGetProperty("known_coverage_gap", out var g) && g.GetBoolean()))];
    }

    /// <summary>
    /// Runs one corpus case end to end: renders the on-screen text back to scan codes, translates
    /// into every layout, and asks the detection engine for a verdict.
    /// </summary>
    private static CorrectionVerdict Evaluate(
        CorpusCase corpusCase,
        IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> keyMaps,
        IReadOnlyList<DictionarySnapshot> dictionaries,
        CautionLevel caution)
    {
        var typedIn = new LayoutId(corpusCase.TypedLayout);
        var typedMap = keyMaps[typedIn];

        var scanCodes = new List<int>(corpusCase.OnScreen.Length);
        foreach (var ch in corpusCase.OnScreen)
        {
            var lower = char.ToLowerInvariant(ch);
            var match = typedMap.FirstOrDefault(kv => kv.Value == lower);
            if (match.Value != lower)
            {
                // A character with no key in the typed layout (a capital, a digit). The real
                // pipeline never assembles such a run from scan codes, so skip the case.
                return CorrectionVerdict.Leave(corpusCase.OnScreen);
            }

            scanCodes.Add(match.Key);
        }

        var engine = new MappingEngine(keyMaps);
        var candidates = engine.Translate(scanCodes, typedIn, [.. keyMaps.Keys]);

        return new DetectionEngine().Evaluate(candidates, dictionaries, caution);
    }

    private static (IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> KeyMaps, DictionarySnapshot[] Dictionaries) LoadData()
    {
        var accessor = new DictionaryAccessor(Path.Combine(RepositoryRoot(), "data"));
        return (accessor.LoadKeyMaps(PairId), [accessor.Load(EnUs), accessor.Load(HeIl)]);
    }

    [Fact]
    public void FalseCorrectionRate_IsMeasuredAndReported()
    {
        var (keyMaps, dictionaries) = LoadData();
        var corpus = LoadCorpus();

        // Every case that must NOT be corrected. Correcting any of these is a false correction —
        // the failure SC-001 bounds, and the one the product cannot afford.
        var mustNotCorrect = corpus
            .Where(c => c.Kind is "true_negative" or "ambiguous" or "unknown")
            .ToList();

        var falseCorrections = mustNotCorrect
            .Select(c => (Case: c, Verdict: Evaluate(c, keyMaps, dictionaries, CautionLevel.Balanced)))
            .Where(x => x.Verdict.Outcome == CorrectionOutcome.Correct)
            .ToList();

        var detail = string.Join(
            "; ",
            falseCorrections.Select(x => $"{x.Case.Id} '{x.Case.OnScreen}' -> '{x.Verdict.TextIntended}'"));

        Assert.True(
            falseCorrections.Count == 0,
            $"{falseCorrections.Count} of {mustNotCorrect.Count} must-not-correct cases were corrected: {detail}");
    }

    [Fact]
    public void TruePositives_AreCorrectedToTheIntendedText()
    {
        var (keyMaps, dictionaries) = LoadData();
        var corpus = LoadCorpus();

        var missed = new List<string>();
        var wrong = new List<string>();
        var coverageGaps = new List<string>();

        foreach (var c in corpus.Where(c => c.Kind == "true_positive"))
        {
            var verdict = Evaluate(c, keyMaps, dictionaries, CautionLevel.Balanced);

            if (verdict.Outcome != CorrectionOutcome.Correct)
            {
                // A case marked as a known coverage gap fails because the shipped dictionary lacks
                // the intended word, not because the algorithm decided wrongly. Separating the two
                // keeps this suite a measure of the ALGORITHM while still counting the gap out loud
                // rather than deleting the case to go green.
                (c.KnownCoverageGap ? coverageGaps : missed)
                    .Add($"{c.Id} '{c.OnScreen}' (meant '{c.Intended}')");
            }
            else if (!string.Equals(verdict.TextIntended, c.Intended, StringComparison.Ordinal))
            {
                wrong.Add($"{c.Id} '{c.OnScreen}' -> '{verdict.TextIntended}', expected '{c.Intended}'");
            }
        }

        // A correction to the WRONG text is a defect of the same severity as a false correction:
        // it changes text the user did not want changed. A missed correction is a lesser defect,
        // by the priority bound at the requirements lens — but it is still a defect.
        Assert.True(wrong.Count == 0, $"Corrected to the wrong text: {string.Join("; ", wrong)}");
        Assert.True(
            missed.Count == 0,
            $"Missed corrections the algorithm should have made: {string.Join("; ", missed)}. "
            + $"Known dictionary-coverage gaps (not counted here): {coverageGaps.Count}");
    }

    [Fact]
    public void ConservativeCaution_NeverCorrectsMoreThanBalanced()
    {
        // A monotonicity property: turning caution up must never cause MORE corrections. If this
        // fails, the caution levels do not mean what the settings screen says they mean.
        var (keyMaps, dictionaries) = LoadData();
        var corpus = LoadCorpus();

        var conservative = corpus.Count(c =>
            Evaluate(c, keyMaps, dictionaries, CautionLevel.Conservative).Outcome == CorrectionOutcome.Correct);
        var balanced = corpus.Count(c =>
            Evaluate(c, keyMaps, dictionaries, CautionLevel.Balanced).Outcome == CorrectionOutcome.Correct);
        var aggressive = corpus.Count(c =>
            Evaluate(c, keyMaps, dictionaries, CautionLevel.Aggressive).Outcome == CorrectionOutcome.Correct);

        Assert.True(conservative <= balanced, $"conservative {conservative} > balanced {balanced}");
        Assert.True(balanced <= aggressive, $"balanced {balanced} > aggressive {aggressive}");
    }

    [Fact]
    public void ShippedPacks_DeclareSourceAndLicence()
    {
        // FR-008a enforced as a test rather than a promise: a pack whose provenance is not stated
        // must not load, so it cannot ship unnoticed.
        var (_, dictionaries) = LoadData();

        Assert.All(dictionaries, d => Assert.True(d.WordCount > 0));
    }
}
