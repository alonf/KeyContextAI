namespace KeyContextAI.Core.Model;

/// <summary>
/// An immutable view of one language's word data, handed to the detection engine by a manager.
/// </summary>
/// <remarks>
/// Engines never load data themselves — the strict IDesign rule bound at the architecture lens is
/// that engines make no accessor calls, so the manager reads and passes a snapshot in. That is also
/// what makes the engines testable with no mocks at all.
/// </remarks>
public sealed class DictionarySnapshot
{
    private readonly HashSet<string> _words;
    private readonly IReadOnlyDictionary<string, int> _frequencies;
    private readonly HashSet<string> _neverCorrect;

    /// <summary>Creates a snapshot for one language.</summary>
    /// <param name="language">The language these words belong to.</param>
    /// <param name="words">The recognized words, shipped plus user-added.</param>
    /// <param name="frequencies">Optional word frequencies, used to break ties between candidates.</param>
    /// <param name="neverCorrect">Words the user has affirmed, which are never corrected (FR-009a).</param>
    public DictionarySnapshot(
        LayoutId language,
        IEnumerable<string> words,
        IReadOnlyDictionary<string, int>? frequencies = null,
        IEnumerable<string>? neverCorrect = null)
    {
        ArgumentNullException.ThrowIfNull(words);

        Language = language;
        _words = new HashSet<string>(words, StringComparer.Ordinal);
        _frequencies = frequencies ?? new Dictionary<string, int>(StringComparer.Ordinal);
        _neverCorrect = new HashSet<string>(neverCorrect ?? [], StringComparer.Ordinal);
    }

    /// <summary>The language this snapshot describes.</summary>
    public LayoutId Language { get; }

    /// <summary>How many words the snapshot recognizes.</summary>
    public int WordCount => _words.Count;

    /// <summary>Whether the word is recognized in this language.</summary>
    /// <param name="word">The word to look up. Case is normalized by the caller's convention.</param>
    public bool Contains(string word) => _words.Contains(word);

    /// <summary>
    /// The word's relative frequency, or zero when unknown. Used only to break ties, never to decide
    /// on its own.
    /// </summary>
    /// <param name="word">The word to look up.</param>
    public int FrequencyOf(string word) => _frequencies.TryGetValue(word, out var f) ? f : 0;

    /// <summary>
    /// Whether the user has affirmed this word, by flipping back a correction or by repeated use, in
    /// which case it is never corrected again (FR-009a).
    /// </summary>
    /// <param name="word">The word to check.</param>
    public bool IsNeverCorrect(string word) => _neverCorrect.Contains(word);
}
