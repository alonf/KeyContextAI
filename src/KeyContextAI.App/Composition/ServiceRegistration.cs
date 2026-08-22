using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Engines;
using KeyContextAI.Core.Model;
using KeyContextAI.Platform.Input;
using KeyContextAI.Platform.System;
using KeyContextAI.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace KeyContextAI.App.Composition;

/// <summary>
/// The composition root. Every component is registered behind its interface with a deliberate
/// lifetime, which is what lets a manager be tested with mocked collaborators.
/// </summary>
/// <remarks>
/// Lifetimes are singleton throughout: managers, engines and accessors are all long-lived for the
/// life of the tray application, and none holds per-request state.
/// </remarks>
public static class ServiceRegistration
{
    /// <summary>The layout pair shipped and proven first.</summary>
    private const string DefaultPairId = "en-US<->he-IL";

    /// <summary>
    /// Registers the components that exist in this iteration.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="dataRoot">The directory holding the shipped key maps and dictionaries.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddKeyContextAi(this IServiceCollection services, string dataRoot)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataRoot);

        // Accessors: the only components that touch the outside world.
        services.AddSingleton<IDictionaryAccessor>(_ => new DictionaryAccessor(dataRoot));
        services.AddSingleton<IKeystrokeAccessor, KeystrokeAccessor>();
        services.AddSingleton<IFocusAccessor, FocusAccessor>();

        // Engines: pure algorithms. The mapping engine is constructed from loaded data rather than
        // loading it itself, because engines make no accessor calls.
        services.AddSingleton<IMappingEngine>(provider =>
        {
            var accessor = provider.GetRequiredService<IDictionaryAccessor>();
            return new MappingEngine(accessor.LoadKeyMaps(DefaultPairId));
        });

        services.AddSingleton<IDetectionEngine, DetectionEngine>();

        // Word assembly holds the word in progress, so each consumer gets its own instance rather
        // than sharing one across surfaces.
        services.AddTransient<IWordAssemblyEngine, WordAssemblyEngine>();

        return services;
    }

    /// <summary>
    /// Loads the dictionary snapshots a manager will hand to the detection engine.
    /// </summary>
    /// <param name="accessor">The dictionary accessor.</param>
    /// <param name="languages">The languages to load.</param>
    /// <param name="onRefused">Called with the language and the reason when a pack is refused, so
    /// the caller can surface it to the user rather than failing silently.</param>
    /// <returns>One snapshot per language that loaded successfully.</returns>
    /// <remarks>
    /// A pack that is refused (unknown schema version, missing licence) is skipped rather than
    /// taking the application down with it: the other language pairs still work, and the refusal is
    /// surfaced to the user by the caller.
    /// </remarks>
    public static IReadOnlyList<DictionarySnapshot> LoadDictionaries(
        IDictionaryAccessor accessor,
        IReadOnlyList<LayoutId> languages,
        Action<LayoutId, string>? onRefused = null)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(languages);

        var snapshots = new List<DictionarySnapshot>(languages.Count);

        foreach (var language in languages)
        {
            try
            {
                snapshots.Add(accessor.Load(language));
            }
            catch (DataPackRejectedException ex)
            {
                onRefused?.Invoke(language, ex.Message);
            }
        }

        return snapshots;
    }
}
