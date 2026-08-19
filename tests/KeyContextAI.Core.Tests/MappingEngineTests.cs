using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Covers FR-005 (translate scan codes into every candidate layout) and FR-008 (a language pair is
/// data, not code).
/// </summary>
public sealed class MappingEngineTests
{
    private static IMappingEngine NewEngine() => new MappingEngine(new Dictionary<LayoutId, IReadOnlyDictionary<int, char>>
    {
        [LayoutMaps.EnUs] = LayoutMaps.English,
        [LayoutMaps.HeIl] = LayoutMaps.Hebrew,
    });

    [Fact]
    public void Translate_TypedInEnglish_ProducesTheIntendedHebrewWord()
    {
        // "akuo" on an English layout is what you get when you meant to type "שלום" in Hebrew.
        var scanCodes = LayoutMaps.ScanCodesFor("akuo", LayoutMaps.English);
        Assert.NotNull(scanCodes);

        var candidates = NewEngine().Translate(scanCodes, LayoutMaps.EnUs, [LayoutMaps.HeIl]);

        var hebrew = Assert.Single(candidates, c => c.Layout == LayoutMaps.HeIl);
        Assert.Equal("שלום", hebrew.Text);
        Assert.True(hebrew.IsComplete);
        Assert.False(hebrew.IsAsTyped);
    }

    [Fact]
    public void Translate_AlwaysIncludesTheAsTypedCandidate()
    {
        var scanCodes = LayoutMaps.ScanCodesFor("hello", LayoutMaps.English);
        Assert.NotNull(scanCodes);

        var candidates = NewEngine().Translate(scanCodes, LayoutMaps.EnUs, [LayoutMaps.HeIl]);

        var asTyped = Assert.Single(candidates, c => c.IsAsTyped);
        Assert.Equal("hello", asTyped.Text);
        Assert.Equal(LayoutMaps.EnUs, asTyped.Layout);
    }

    [Fact]
    public void Translate_IsSymmetric_HebrewBackToEnglish()
    {
        var scanCodes = LayoutMaps.ScanCodesFor("שלום", LayoutMaps.Hebrew);
        Assert.NotNull(scanCodes);

        var candidates = NewEngine().Translate(scanCodes, LayoutMaps.HeIl, [LayoutMaps.EnUs]);

        var english = Assert.Single(candidates, c => c.Layout == LayoutMaps.EnUs);
        Assert.Equal("akuo", english.Text);
    }

    [Fact]
    public void Translate_UnmappableScanCode_ReturnsAnIncompleteCandidateRatherThanThrowing()
    {
        // 9999 exists in no layout. The candidate must come back marked incomplete rather than
        // being omitted, so the caller can tell "no mapping" from "no such layout".
        var candidates = NewEngine().Translate([9999], LayoutMaps.EnUs, [LayoutMaps.HeIl]);

        var hebrew = Assert.Single(candidates, c => c.Layout == LayoutMaps.HeIl);
        Assert.False(hebrew.IsComplete);
    }

    [Fact]
    public void Translate_UnknownTargetLayout_IsSkippedNotThrown()
    {
        var scanCodes = LayoutMaps.ScanCodesFor("akuo", LayoutMaps.English);
        Assert.NotNull(scanCodes);

        var candidates = NewEngine().Translate(scanCodes, LayoutMaps.EnUs, [new LayoutId("fr-FR")]);

        Assert.DoesNotContain(candidates, c => c.Layout == new LayoutId("fr-FR"));
    }

    [Fact]
    public void Translate_EmptyInput_ReturnsNoCandidates()
    {
        Assert.Empty(NewEngine().Translate([], LayoutMaps.EnUs, [LayoutMaps.HeIl]));
    }

    [Fact]
    public void Translate_IsDeterministic()
    {
        var scanCodes = LayoutMaps.ScanCodesFor("akuo", LayoutMaps.English);
        Assert.NotNull(scanCodes);
        var engine = NewEngine();

        var first = engine.Translate(scanCodes, LayoutMaps.EnUs, [LayoutMaps.HeIl]);
        var second = engine.Translate(scanCodes, LayoutMaps.EnUs, [LayoutMaps.HeIl]);

        Assert.Equal(first.Select(c => c.Text), second.Select(c => c.Text));
    }

    [Fact]
    public void Translate_ANewPairIsDataOnly()
    {
        // FR-008: adding a language pair must not require new code. A third layout added purely as
        // data must translate without touching the engine.
        var greek = new LayoutId("el-GR");
        var greekMap = LayoutMaps.English.ToDictionary(kv => kv.Key, kv => (char)('α' + (kv.Key - 100)));
        var engine = new MappingEngine(new Dictionary<LayoutId, IReadOnlyDictionary<int, char>>
        {
            [LayoutMaps.EnUs] = LayoutMaps.English,
            [greek] = greekMap,
        });

        var scanCodes = LayoutMaps.ScanCodesFor("ab", LayoutMaps.English);
        Assert.NotNull(scanCodes);

        var candidates = engine.Translate(scanCodes, LayoutMaps.EnUs, [greek]);

        Assert.Single(candidates, c => c.Layout == greek && c.IsComplete);
    }
}
