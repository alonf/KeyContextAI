using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>What appending a keystroke did to the word in progress.</summary>
public enum WordAssemblyOutcome
{
    /// <summary>The keystroke did not change the word in progress.</summary>
    NoChange,

    /// <summary>The keystroke extended or shortened the word in progress.</summary>
    WordInProgress,

    /// <summary>The keystroke ended the word, which is now complete and ready to evaluate.</summary>
    WordCompleted,
}

/// <summary>
/// The result of appending one keystroke to the word in progress.
/// </summary>
/// <param name="Outcome">What the keystroke did.</param>
/// <param name="Text">The word's text. Empty unless the word is in progress or completed.</param>
/// <param name="ScanCodes">The word's scan codes in typing order, retained so that re-mapping does
/// not depend on the characters the active layout produced.</param>
/// <param name="CompletedByCommittingKey">True when the word ended on a key such as Enter that may
/// also submit the input, which is the case the correction path must handle before the key reaches
/// the application (FR-005b).</param>
public sealed record WordAssemblyResult(
    WordAssemblyOutcome Outcome,
    string Text,
    IReadOnlyList<int> ScanCodes,
    bool CompletedByCommittingKey);

/// <summary>
/// Collects keystrokes into a word and recognizes when that word is complete.
/// </summary>
/// <remarks>
/// A pure engine holding only the word in progress. A word completes on a separator or a committing
/// key and never mid-word, because word boundaries are not knowable while typing (FR-005b).
/// </remarks>
public interface IWordAssemblyEngine
{
    /// <summary>
    /// Appends a keystroke to the word in progress.
    /// </summary>
    /// <param name="key">The observed keystroke.</param>
    /// <returns>What the keystroke did, and the word when there is one. Never throws.</returns>
    WordAssemblyResult Append(KeyEvent key);

    /// <summary>
    /// Discards the word in progress. Called on focus change, on a password field, and on wipe.
    /// </summary>
    void Reset();
}
