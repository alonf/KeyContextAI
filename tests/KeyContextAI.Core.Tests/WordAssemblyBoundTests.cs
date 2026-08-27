using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// The hardening gate sets an explicit maximum of 256 characters per active focus session. Without
/// enforcement a long boundary-free input retains arbitrary typed text in memory.
/// </summary>
public sealed class WordAssemblyBoundTests
{
    [Fact]
    public void WordAtTheBound_IsStillAssembled()
    {
        var engine = new WordAssemblyEngine();

        WordAssemblyResult last = default!;
        for (var i = 0; i < WordAssemblyEngine.MaxWordLength; i++)
        {
            last = engine.Append(Character('a'));
        }

        Assert.Equal(WordAssemblyOutcome.WordInProgress, last.Outcome);
        Assert.Equal(WordAssemblyEngine.MaxWordLength, last.Text.Length);
    }

    [Fact]
    public void ExceedingTheBound_DiscardsTheWordRatherThanTruncatingIt()
    {
        var engine = new WordAssemblyEngine();

        for (var i = 0; i < WordAssemblyEngine.MaxWordLength; i++)
        {
            engine.Append(Character('a'));
        }

        var overflow = engine.Append(Character('b'));

        // Truncation would leave a word that no longer matches the text on screen, so a
        // correction computed from it would replace the wrong span.
        Assert.Equal(WordAssemblyOutcome.NoChange, overflow.Outcome);
        Assert.Empty(overflow.Text);
    }

    [Fact]
    public void AfterOverflow_NoCompletedWordIsOffered()
    {
        var engine = new WordAssemblyEngine();

        for (var i = 0; i < WordAssemblyEngine.MaxWordLength + 1; i++)
        {
            engine.Append(Character('a'));
        }

        var completed = engine.Append(Separator());

        Assert.Equal(WordAssemblyOutcome.NoChange, completed.Outcome);
    }

    [Fact]
    public void RetainedTextNeverExceedsTheBound()
    {
        var engine = new WordAssemblyEngine();

        WordAssemblyResult last = default!;
        for (var i = 0; i < WordAssemblyEngine.MaxWordLength * 4; i++)
        {
            last = engine.Append(Character('a'));
        }

        Assert.True(last.Text.Length <= WordAssemblyEngine.MaxWordLength);
    }

    private static KeyEvent Character(char value) =>
        new(30, 65, value, new LayoutId("en-US"), KeyEventKind.Character, false, 0);

    private static KeyEvent Separator() =>
        new(57, 32, ' ', new LayoutId("en-US"), KeyEventKind.Separator, false, 0);
}
