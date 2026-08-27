using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Injects text back into the foreground application.
/// </summary>
/// <remarks>
/// A resource accessor: it touches the outside world and calls nothing inside the system. It is
/// responsible for injecting a correction burst and, when needed, re-delivering the suppressed key
/// on the compensating path.
/// </remarks>
public interface IInputInjectionAccessor
{
    /// <summary>Applies a correction as one native input burst.</summary>
    Task<InjectionResult> ApplyCorrectionAsync(CorrectionTransaction tx);

    /// <summary>Delivers a suppressed key alone.</summary>
    Task ReinjectKeyAsync(KeyEvent key);
}
