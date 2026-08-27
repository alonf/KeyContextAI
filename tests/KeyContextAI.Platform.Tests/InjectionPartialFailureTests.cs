using KeyContextAI.Core.Model;
using KeyContextAI.Platform.Input;

namespace KeyContextAI.Platform.Tests;

/// <summary>
/// A burst that fails after some events have landed has already changed the user's document. The
/// result must carry enough information to undo that prefix, or the documented control that an
/// injection failure leaves the original text intact does not hold.
/// </summary>
public sealed class InjectionPartialFailureTests
{
    [Fact]
    public void Failure_ReportsNothingAppliedSoNoCompensationIsNeeded()
    {
        var result = InjectionResult.Failure("rejected");

        Assert.False(result.Succeeded);
        Assert.Equal(InjectionFailureKind.NothingApplied, result.FailureKind);
        Assert.Equal(0, result.AppliedEventCount);
        Assert.Equal(string.Empty, result.AppliedReplacementText);
    }

    [Fact]
    public void PartialFailure_CarriesTheAppliedPrefix()
    {
        var result = InjectionResult.PartialFailure("stopped short", 6, 2, "ab");

        Assert.False(result.Succeeded);
        Assert.Equal(InjectionFailureKind.PartiallyApplied, result.FailureKind);
        Assert.Equal(6, result.AppliedEventCount);
        Assert.Equal(2, result.AppliedBackspaceCount);
        Assert.Equal("ab", result.AppliedReplacementText);
    }

    [Fact]
    public void Success_CarriesNoFailureAccounting()
    {
        var result = InjectionResult.Success();

        Assert.True(result.Succeeded);
        Assert.Equal(InjectionFailureKind.None, result.FailureKind);
    }

    [Fact]
    public void OriginalTextPrefix_RecoversTheCharactersTheBackspacesRemoved()
    {
        var tx = TransactionWithSpan("hello", "world");

        Assert.Equal("rld", InputInjectionAccessor.OriginalTextPrefixForTest(tx, 3));
    }

    [Fact]
    public void OriginalTextPrefix_ClampsToTheAvailableSpan()
    {
        var tx = TransactionWithSpan("ab");

        Assert.Equal("ab", InputInjectionAccessor.OriginalTextPrefixForTest(tx, 99));
    }

    [Fact]
    public void OriginalTextPrefix_IsEmptyWhenNoBackspacesWereApplied()
    {
        var tx = TransactionWithSpan("abc");

        Assert.Equal(string.Empty, InputInjectionAccessor.OriginalTextPrefixForTest(tx, 0));
    }

    [Fact]
    public void OddPrefixEndingOnBackspaceKeydown_CountsThatDeletion()
    {
        // Steps: BS-down, BS-up, BS-down, BS-up, 'x'-down, 'x'-up. Stopping after 5 means two
        // backspaces completed and the 'x' keydown already inserted a character.
        var result = InputInjectionAccessor.SendCorrectionBurstForTest(2, "x", _ => 5);

        Assert.Equal(InjectionFailureKind.PartiallyApplied, result.FailureKind);
        Assert.Equal(2, result.AppliedBackspaceCount);
        Assert.Equal("x", result.AppliedReplacementText);
    }

    [Fact]
    public void OddPrefixStoppingMidBackspace_StillCountsTheDeletion()
    {
        // Stopping after 3 events means BS-down, BS-up, BS-down: the third keydown has already
        // deleted a character even though its keyup never ran. Halving the count would miss it.
        var result = InputInjectionAccessor.SendCorrectionBurstForTest(3, "ab", _ => 3);

        Assert.Equal(InjectionFailureKind.PartiallyApplied, result.FailureKind);
        Assert.Equal(2, result.AppliedBackspaceCount);
        Assert.Equal(string.Empty, result.AppliedReplacementText);
    }

    [Fact]
    public void PrefixCoveringPartOfTheReplacement_CountsOnlyTheInsertedCharacters()
    {
        // 1 backspace (2 events) then 'a','b','c' as pairs. Stopping after 6 means 'a' and 'b'
        // were inserted and 'c' was not.
        var result = InputInjectionAccessor.SendCorrectionBurstForTest(1, "abc", _ => 6);

        Assert.Equal(1, result.AppliedBackspaceCount);
        Assert.Equal("ab", result.AppliedReplacementText);
    }

    [Fact]
    public void CompleteBurst_Succeeds()
    {
        var result = InputInjectionAccessor.SendCorrectionBurstForTest(1, "a", inputs => inputs.Length);

        Assert.True(result.Succeeded);
        Assert.Equal(InjectionFailureKind.None, result.FailureKind);
    }

    [Fact]
    public void BurstRejectedOutright_ReportsNothingApplied()
    {
        var result = InputInjectionAccessor.SendCorrectionBurstForTest(2, "ab", _ => 0);

        Assert.Equal(InjectionFailureKind.NothingApplied, result.FailureKind);
        Assert.Equal(0, result.AppliedBackspaceCount);
    }

    private static CorrectionTransaction TransactionWithSpan(params string[] words)
    {
        var entries = words
            .Select(word => new TranscriptEntry(
                Guid.NewGuid(),
                word,
                [],
                new LayoutId("en-US"),
                0,
                TranscriptEntryState.Complete,
                null,
                0))
            .ToList();

        return new CorrectionTransaction(
            Guid.NewGuid(),
            entries.Sum(entry => entry.Text.Length),
            "replacement",
            new LayoutId("he-IL"),
            null,
            new IntPtr(1),
            entries);
    }
}
