namespace KeyContextAI.Core.Model;

/// <summary>
/// Identifies an installed keyboard layout by its BCP-47 locale tag, for example <c>en-US</c> or
/// <c>he-IL</c>.
/// </summary>
/// <remarks>
/// A value type rather than a bare string so a layout cannot be confused with a language name or a
/// dictionary key at a call site, per the stronger-domain-types rule bound at the code lens.
/// </remarks>
public readonly record struct LayoutId
{
    private readonly string? _tag;

    /// <summary>Creates a layout identifier from its locale tag.</summary>
    /// <param name="tag">A non-empty locale tag such as <c>he-IL</c>.</param>
    /// <exception cref="ArgumentException">The tag is null, empty, or whitespace.</exception>
    public LayoutId(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("A layout tag must be a non-empty locale tag.", nameof(tag));
        }

        _tag = tag;
    }

    /// <summary>The locale tag. Never null or empty for a constructed value.</summary>
    public string Tag => _tag ?? throw new InvalidOperationException(
        "This LayoutId was never constructed; use the constructor rather than default(LayoutId).");

    /// <inheritdoc />
    public override string ToString() => Tag;
}
