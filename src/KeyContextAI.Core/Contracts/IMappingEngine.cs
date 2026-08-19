using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Translates a run of scan codes into the text each candidate layout would have produced.
/// </summary>
/// <remarks>
/// A pure engine: deterministic, side-effect free, and making no accessor calls. Translation works
/// from scan codes rather than from produced characters, so the result does not depend on what the
/// active layout happened to render.
/// </remarks>
public interface IMappingEngine
{
    /// <summary>
    /// Produces one candidate per target layout, plus the text as typed.
    /// </summary>
    /// <param name="scanCodes">The scan codes of the run, in typing order.</param>
    /// <param name="typedIn">The layout that was active while the run was typed.</param>
    /// <param name="targets">The layouts to translate into. The layout in <paramref name="typedIn"/>
    /// may be included and is returned as the as-typed candidate.</param>
    /// <returns>
    /// One candidate per distinct layout. A candidate whose scan codes are not all mappable is
    /// returned with <see cref="Candidate.IsComplete"/> false rather than omitted, so callers can
    /// tell "no mapping" apart from "no such layout". Never throws.
    /// </returns>
    IReadOnlyList<Candidate> Translate(
        IReadOnlyList<int> scanCodes,
        LayoutId typedIn,
        IReadOnlyList<LayoutId> targets);
}
