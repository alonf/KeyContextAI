using System.Text.Json;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Platform.Storage;

/// <inheritdoc cref="IDictionaryAccessor" />
public sealed class DictionaryAccessor : IDictionaryAccessor
{
    /// <summary>The only data-file schema version this build understands.</summary>
    private const int SupportedSchemaVersion = 1;

    private readonly string _dataRoot;

    /// <summary>Creates the accessor over a data directory.</summary>
    /// <param name="dataRoot">The directory holding <c>keymaps/</c> and <c>dictionaries/</c>.</param>
    public DictionaryAccessor(string dataRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataRoot);
        _dataRoot = dataRoot;
    }

    /// <inheritdoc />
    public DictionarySnapshot Load(LayoutId language)
    {
        var packDirectory = Path.Combine(_dataRoot, "dictionaries", language.Tag);
        var manifestPath = Path.Combine(packDirectory, "pack.json");

        var manifest = ReadJson(manifestPath, "dictionary pack manifest");
        RequireSupportedSchema(manifest, manifestPath);

        // FR-008a: a pack that cannot state where it came from and under what licence does not ship.
        var source = ReadRequiredString(manifest, "source", manifestPath);
        var licence = ReadRequiredString(manifest, "licence", manifestPath);
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(licence))
        {
            throw new DataPackRejectedException(
                $"The dictionary pack at '{manifestPath}' must declare both a source and a licence.");
        }

        var wordsFile = ReadRequiredString(manifest, "words_file", manifestPath);
        var wordsPath = Path.Combine(packDirectory, wordsFile);
        if (!File.Exists(wordsPath))
        {
            throw new DataPackRejectedException(
                $"The dictionary pack at '{manifestPath}' names a words file '{wordsFile}' that does not exist.");
        }

        var words = File.ReadAllLines(wordsPath)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'));

        return new DictionarySnapshot(language, words);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<LayoutId, IReadOnlyDictionary<int, char>> LoadKeyMaps(string pairId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pairId);

        // The pair id uses "<->" which is not a legal filename character on Windows.
        var fileName = pairId.Replace("<->", "_", StringComparison.Ordinal) + ".json";
        var mapPath = Path.Combine(_dataRoot, "keymaps", fileName);

        var document = ReadJson(mapPath, "key map");
        RequireSupportedSchema(document, mapPath);

        if (!document.TryGetProperty("keys", out var keys) || keys.ValueKind != JsonValueKind.Array)
        {
            throw new DataPackRejectedException($"The key map at '{mapPath}' has no 'keys' array.");
        }

        var maps = new Dictionary<LayoutId, Dictionary<int, char>>();

        foreach (var key in keys.EnumerateArray())
        {
            if (!key.TryGetProperty("scan", out var scanElement) || !scanElement.TryGetInt32(out var scan))
            {
                throw new DataPackRejectedException($"The key map at '{mapPath}' has an entry without a numeric 'scan'.");
            }

            foreach (var property in key.EnumerateObject())
            {
                if (property.NameEquals("scan") || property.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var rendered = property.Value.GetString();
                if (string.IsNullOrEmpty(rendered))
                {
                    continue;
                }

                var layout = new LayoutId(property.Name);
                if (!maps.TryGetValue(layout, out var map))
                {
                    map = [];
                    maps[layout] = map;
                }

                map[scan] = rendered[0];
            }
        }

        if (maps.Count == 0)
        {
            throw new DataPackRejectedException($"The key map at '{mapPath}' declares no layouts.");
        }

        return maps.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyDictionary<int, char>)kv.Value);
    }

    private static JsonElement ReadJson(string path, string what)
    {
        if (!File.Exists(path))
        {
            throw new DataPackRejectedException($"No {what} was found at '{path}'.");
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new DataPackRejectedException($"The {what} at '{path}' is not valid JSON.", ex);
        }
    }

    /// <summary>
    /// Refuses a file whose schema version this build does not understand, rather than reading it
    /// on a best-effort basis (FR-029).
    /// </summary>
    private static void RequireSupportedSchema(JsonElement root, string path)
    {
        if (!root.TryGetProperty("schema_version", out var element) || !element.TryGetInt32(out var version))
        {
            throw new DataPackRejectedException($"The file at '{path}' declares no schema_version.");
        }

        if (version != SupportedSchemaVersion)
        {
            throw new DataPackRejectedException(
                $"The file at '{path}' declares schema_version {version}, but this build understands "
                + $"only version {SupportedSchemaVersion}. Refusing it rather than guessing at its contents.");
        }
    }

    private static string ReadRequiredString(JsonElement root, string propertyName, string path)
    {
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
        {
            throw new DataPackRejectedException($"The file at '{path}' has no '{propertyName}' value.");
        }

        return element.GetString() ?? string.Empty;
    }
}
