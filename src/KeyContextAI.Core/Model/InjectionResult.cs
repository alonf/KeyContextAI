namespace KeyContextAI.Core.Model;

/// <summary>
/// Reports whether an injection burst succeeded, and when it did not, how much of it was applied.
/// </summary>
/// <param name="Succeeded">True when every event in the burst was inserted.</param>
/// <param name="ErrorMessage">Why the burst failed, when it failed.</param>
/// <param name="FailureKind">Whether a failed burst left the target text untouched or mutated.</param>
/// <param name="AppliedEventCount">How many native events were inserted before the burst stopped.
/// A partial burst has already changed the user's document, so this is what a compensating path
/// works from.</param>
/// <param name="AppliedBackspaceCount">How many of the applied events were backspaces that
/// removed existing text.</param>
/// <param name="AppliedReplacementText">The prefix of the replacement text that was inserted.</param>
public sealed record InjectionResult(
    bool Succeeded,
    string? ErrorMessage,
    InjectionFailureKind FailureKind = InjectionFailureKind.None,
    int AppliedEventCount = 0,
    int AppliedBackspaceCount = 0,
    string AppliedReplacementText = "")
{
    /// <summary>A successful injection.</summary>
    public static InjectionResult Success() => new(true, null);

    /// <summary>A failed injection that inserted nothing, leaving the original text intact.</summary>
    public static InjectionResult Failure(string errorMessage) =>
        new(false, errorMessage, InjectionFailureKind.NothingApplied);

    /// <summary>
    /// An abandoned injection that never reached the target, so the user's text is untouched.
    /// </summary>
    public static InjectionResult Abandoned(string errorMessage) =>
        new(false, errorMessage, InjectionFailureKind.TargetLost);

    /// <summary>
    /// A failed injection that had already mutated the target text. The caller must compensate
    /// for the applied prefix rather than assume the document is unchanged.
    /// </summary>
    public static InjectionResult PartialFailure(
        string errorMessage,
        int appliedEventCount,
        int appliedBackspaceCount,
        string appliedReplacementText) =>
        new(
            false,
            errorMessage,
            InjectionFailureKind.PartiallyApplied,
            appliedEventCount,
            appliedBackspaceCount,
            appliedReplacementText);
}
