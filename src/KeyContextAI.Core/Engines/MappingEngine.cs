using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Engines;

/// <inheritdoc cref="IMappingEngine" />
public sealed class MappingEngine : IMappingEngine
{
    private readonly IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> _layouts;

    /// <summary>Creates the engine over the supplied scan-code maps.</summary>
    /// <param name="layouts">One scan-code-to-character map per known layout. Supplied by the
    /// manager from loaded data, because engines make no accessor calls.</param>
    public MappingEngine(IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        _layouts = layouts;
    }

    /// <inheritdoc />
    public IReadOnlyList<Candidate> Translate(
        IReadOnlyList<int> scanCodes,
        LayoutId typedIn,
        IReadOnlyList<LayoutId> targets)
    {
        ArgumentNullException.ThrowIfNull(scanCodes);
        ArgumentNullException.ThrowIfNull(targets);

        if (scanCodes.Count == 0)
        {
            return [];
        }

        var candidates = new List<Candidate>(targets.Count + 1);

        if (TryRender(scanCodes, typedIn, out var asTypedText, out var asTypedComplete))
        {
            candidates.Add(new Candidate(typedIn, asTypedText, asTypedComplete, IsAsTyped: true));
        }

        foreach (var target in targets)
        {
            if (target == typedIn || !TryRender(scanCodes, target, out var text, out var isComplete))
            {
                continue;
            }

            candidates.Add(new Candidate(target, text, isComplete, IsAsTyped: false));
        }

        return candidates;
    }

    /// <summary>
    /// Renders the scan codes under one layout. Returns false only when the layout is unknown; an
    /// unmappable code yields a rendered candidate flagged incomplete rather than a failure, so the
    /// caller can distinguish "no mapping for these keys" from "no such layout".
    /// </summary>
    private bool TryRender(IReadOnlyList<int> scanCodes, LayoutId layout, out string text, out bool isComplete)
    {
        text = string.Empty;
        isComplete = false;

        if (!_layouts.TryGetValue(layout, out var map))
        {
            return false;
        }

        var buffer = new char[scanCodes.Count];
        var complete = true;

        for (var i = 0; i < scanCodes.Count; i++)
        {
            if (map.TryGetValue(scanCodes[i], out var ch))
            {
                buffer[i] = ch;
            }
            else
            {
                buffer[i] = '�';
                complete = false;
            }
        }

        text = new string(buffer);
        isComplete = complete;
        return true;
    }
}
