using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

/// <summary>
/// Hand-built scan-code maps for the tests, so engine tests need no files and no mocks.
/// </summary>
/// <remarks>
/// Only the letter rows are covered — enough to type the words the tests use. The real maps ship as
/// data under <c>data/keymaps/</c>.
/// </remarks>
internal static class LayoutMaps
{
    internal static readonly LayoutId EnUs = new("en-US");
    internal static readonly LayoutId HeIl = new("he-IL");

    /// <summary>The US-English letters in scan-code order across the three letter rows.</summary>
    private const string EnglishRow = "qwertyuiop"
                                    + "asdfghjkl;"
                                    + "zxcvbnm,./";

    /// <summary>
    /// The Hebrew letters on the same physical keys, in the same order. Both rows must be the same
    /// length: index N in each string is the same physical key.
    /// </summary>
    private const string HebrewRow = "/'קראטוןםפ"
                                   + "שדגכעיחלךף"
                                   + "זסבהנמצתץ.";

    /// <summary>
    /// Scan codes for the letter rows. Values are arbitrary but stable, since translation only ever
    /// works from the code, never from what it renders.
    /// </summary>
    internal static IReadOnlyDictionary<int, char> English { get; } = BuildRow(EnglishRow);

    internal static IReadOnlyDictionary<int, char> Hebrew { get; } = BuildRow(HebrewRow);

    /// <summary>Maps a word typed in one layout back to the scan codes that produced it.</summary>
    /// <param name="text">The text as it appeared on screen.</param>
    /// <param name="layoutMap">The layout that rendered it.</param>
    /// <returns>The scan codes, or null when a character has no key in that layout.</returns>
    internal static IReadOnlyList<int>? ScanCodesFor(string text, IReadOnlyDictionary<int, char> layoutMap)
    {
        var codes = new List<int>(text.Length);
        foreach (var ch in text)
        {
            var match = layoutMap.FirstOrDefault(kv => kv.Value == ch);
            if (match.Value != ch)
            {
                return null;
            }

            codes.Add(match.Key);
        }

        return codes;
    }

    /// <summary>Both rows describe the same physical keys, so a length mismatch is a data bug.</summary>
    internal static int KeyCount => EnglishRow.Length;

    private static Dictionary<int, char> BuildRow(string letters)
    {
        if (letters.Length != EnglishRow.Length)
        {
            throw new InvalidOperationException(
                $"Layout rows must describe the same {EnglishRow.Length} keys; got {letters.Length}.");
        }

        var map = new Dictionary<int, char>(letters.Length);
        for (var i = 0; i < letters.Length; i++)
        {
            map[100 + i] = letters[i];
        }

        return map;
    }
}
