using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Covers FR-005b: a word completes on a separator or a committing key, and never mid-word, because
/// word boundaries are not knowable while typing.
/// </summary>
public sealed class WordAssemblyEngineTests
{
    private static KeyEvent Key(char c, KeyEventKind kind = KeyEventKind.Character, int scanCode = 0) =>
        new(scanCode == 0 ? 100 + c : scanCode, c, c, LayoutMaps.EnUs, kind, IsSelfInjected: false, TimestampTicks: 0);

    private static WordAssemblyResult TypeAll(IWordAssemblyEngine engine, string text)
    {
        WordAssemblyResult result = new(WordAssemblyOutcome.NoChange, string.Empty, [], false);
        foreach (var c in text)
        {
            result = engine.Append(Key(c));
        }

        return result;
    }

    [Fact]
    public void Append_MidWord_NeverReportsCompletion()
    {
        var engine = new WordAssemblyEngine();

        var result = TypeAll(engine, "akuo");

        // The whole point of FR-005b: no completion until a boundary arrives.
        Assert.Equal(WordAssemblyOutcome.WordInProgress, result.Outcome);
        Assert.Equal("akuo", result.Text);
    }

    [Fact]
    public void Append_Space_CompletesTheWord()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key(' ', KeyEventKind.Separator));

        Assert.Equal(WordAssemblyOutcome.WordCompleted, result.Outcome);
        Assert.Equal("akuo", result.Text);
        Assert.False(result.CompletedByCommittingKey);
    }

    [Fact]
    public void Append_Punctuation_CompletesTheWord()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key('.', KeyEventKind.Separator));

        Assert.Equal(WordAssemblyOutcome.WordCompleted, result.Outcome);
        Assert.Equal("akuo", result.Text);
    }

    [Fact]
    public void Append_CommittingKey_CompletesTheWordAndSaysSo()
    {
        // The chat-send case. The caller needs to know the key may also submit the input, because
        // that is the path where the key is briefly withheld.
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key('\r', KeyEventKind.Committing));

        Assert.Equal(WordAssemblyOutcome.WordCompleted, result.Outcome);
        Assert.Equal("akuo", result.Text);
        Assert.True(result.CompletedByCommittingKey);
    }

    [Fact]
    public void Append_RetainsScanCodesNotJustCharacters()
    {
        // Re-mapping must not depend on what the active layout rendered.
        var engine = new WordAssemblyEngine();
        engine.Append(Key('a', scanCode: 111));
        var result = engine.Append(Key('b', scanCode: 222));

        Assert.Equal([111, 222], result.ScanCodes);
    }

    [Fact]
    public void Append_Backspace_ShortensTheWord()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key('\b', KeyEventKind.Editing));

        Assert.Equal(WordAssemblyOutcome.WordInProgress, result.Outcome);
        Assert.Equal("aku", result.Text);
    }

    [Fact]
    public void Append_BackspaceOnEmptyWord_IsHarmless()
    {
        var result = new WordAssemblyEngine().Append(Key('\b', KeyEventKind.Editing));

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void Append_SeparatorWithNoWordInProgress_CompletesNothing()
    {
        // Pressing space twice must not report an empty word as a completion to evaluate.
        var engine = new WordAssemblyEngine();
        engine.Append(Key(' ', KeyEventKind.Separator));

        var result = engine.Append(Key(' ', KeyEventKind.Separator));

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
    }

    [Fact]
    public void Append_Modifier_LeavesTheWordUntouched()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key('\0', KeyEventKind.Modifier));

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
        Assert.Equal("akuo", result.Text);
    }

    [Fact]
    public void Append_NavigationKey_AbandonsTheWordInProgress()
    {
        // The caret may have moved anywhere, so the accumulated word no longer describes what is on
        // screen and must not be corrected.
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        var result = engine.Append(Key('\0', KeyEventKind.Other));

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void Append_SelfInjectedKey_IsIgnored()
    {
        // FR-013: our own injected keystrokes must never re-enter the pipeline, or a correction
        // could trigger a correction.
        var engine = new WordAssemblyEngine();
        var injected = new KeyEvent(100, 'x', 'x', LayoutMaps.EnUs, KeyEventKind.Character, IsSelfInjected: true, 0);

        var result = engine.Append(injected);

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void Reset_DiscardsTheWordInProgress()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");

        engine.Reset();
        var result = engine.Append(Key(' ', KeyEventKind.Separator));

        Assert.Equal(WordAssemblyOutcome.NoChange, result.Outcome);
    }

    [Fact]
    public void Append_AfterCompletion_StartsAFreshWord()
    {
        var engine = new WordAssemblyEngine();
        TypeAll(engine, "akuo");
        engine.Append(Key(' ', KeyEventKind.Separator));

        var result = TypeAll(engine, "hi");

        Assert.Equal("hi", result.Text);
    }
}
