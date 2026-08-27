namespace KeyContextAI.Core.Model;

/// <summary>
/// How far an injection burst got, which decides whether the user's text was left intact.
/// </summary>
public enum InjectionFailureKind
{
    /// <summary>The burst succeeded in full.</summary>
    None,

    /// <summary>The burst was rejected before any event reached the target, so the original text
    /// is untouched and no compensation is required.</summary>
    NothingApplied,

    /// <summary>Some events were applied before the burst failed. The user's text has already been
    /// mutated and the caller must compensate for the applied prefix.</summary>
    PartiallyApplied,
}
