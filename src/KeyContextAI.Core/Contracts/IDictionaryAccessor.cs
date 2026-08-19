using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Raised when a data file cannot be trusted: an unknown schema version, a missing licence
/// declaration, or a malformed manifest.
/// </summary>
/// <remarks>
/// Loudly refusing a file is the required behaviour (FR-029). Silently misreading data the tool
/// then types into someone's document is the failure this exception exists to prevent.
/// </remarks>
public sealed class DataPackRejectedException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="message">Why the pack was refused, in terms a user could act on.</param>
    public DataPackRejectedException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">Why the pack was refused.</param>
    /// <param name="innerException">The underlying parse or IO failure.</param>
    public DataPackRejectedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception.</summary>
    public DataPackRejectedException() : base("A data pack was refused.")
    {
    }
}

/// <summary>
/// Loads keyboard maps and dictionaries from disk.
/// </summary>
/// <remarks>
/// A resource accessor: it decouples the system from the file system and calls nothing inside the
/// system. Engines never call it — a manager loads and passes snapshots in.
/// </remarks>
public interface IDictionaryAccessor
{
    /// <summary>Loads one language's word data into memory.</summary>
    /// <param name="language">The language to load.</param>
    /// <returns>An immutable snapshot the detection engine can be handed.</returns>
    /// <exception cref="DataPackRejectedException">The pack is missing, malformed, carries an
    /// unrecognized schema version, or does not declare its source and licence.</exception>
    DictionarySnapshot Load(LayoutId language);

    /// <summary>Loads the scan-code maps for a layout pair.</summary>
    /// <param name="pairId">The pair identifier, for example <c>en-US&lt;-&gt;he-IL</c>.</param>
    /// <returns>One scan-code-to-character map per layout in the pair.</returns>
    /// <exception cref="DataPackRejectedException">The map file is missing, malformed, or carries an
    /// unrecognized schema version.</exception>
    IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> LoadKeyMaps(string pairId);
}
