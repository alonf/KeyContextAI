using KeyContextAI.Core.Model;
using KeyContextAI.Platform.Input;

namespace KeyContextAI.Platform.Tests;

/// <summary>
/// Key classification decides where words end and how far back a correction reaches, so a
/// misclassified key produces a correction span that no longer matches the text on screen.
/// </summary>
public sealed class KeystrokeClassificationTests
{
    [Theory]
    [InlineData(0x0D)] // Enter
    [InlineData(0x09)] // Tab - FR-005b defines Tab as committing, not a separator
    public void CommittingKeys_AreClassifiedAsCommitting(uint virtualKey) =>
        Assert.Equal(KeyEventKind.Committing, KeystrokeAccessor.ClassifyKeyForTest(virtualKey));

    [Theory]
    [InlineData(0x20)] // Space
    [InlineData(0xBC)] // Comma
    [InlineData(0xBE)] // Period
    [InlineData(0xBA)] // Semicolon
    [InlineData(0xDB)] // Open bracket
    public void SeparatorKeys_EndTheWord(uint virtualKey) =>
        Assert.Equal(KeyEventKind.Separator, KeystrokeAccessor.ClassifyKeyForTest(virtualKey));

    [Fact]
    public void Backspace_ShortensTheWordInProgress() =>
        Assert.Equal(KeyEventKind.Editing, KeystrokeAccessor.ClassifyKeyForTest(0x08));

    [Fact]
    public void ForwardDelete_IsNotEditingBecauseItRemovesTextAheadOfTheCaret() =>
        Assert.Equal(KeyEventKind.Other, KeystrokeAccessor.ClassifyKeyForTest(0x2E));

    [Theory]
    [InlineData(0x25)] // Left arrow
    [InlineData(0x27)] // Right arrow
    [InlineData(0x24)] // Home
    [InlineData(0x1B)] // Escape
    [InlineData(0x70)] // F1
    public void NavigationAndFunctionKeys_ResetTheWordInProgress(uint virtualKey) =>
        Assert.Equal(KeyEventKind.Other, KeystrokeAccessor.ClassifyKeyForTest(virtualKey));

    [Theory]
    [InlineData(0x10)] // Shift
    [InlineData(0x11)] // Control
    public void ModifierKeys_ContributeNoCharacter(uint virtualKey) =>
        Assert.Equal(KeyEventKind.Modifier, KeystrokeAccessor.ClassifyKeyForTest(virtualKey));

    [Fact]
    public void LetterKeys_RemainCharacters() =>
        Assert.Equal(KeyEventKind.Character, KeystrokeAccessor.ClassifyKeyForTest(0x41));
}
