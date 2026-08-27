namespace KeyContextAI.Core.Model;

/// <summary>Reports whether an injection burst succeeded.</summary>
public sealed record InjectionResult(bool Succeeded, string? ErrorMessage)
{
    /// <summary>A successful injection.</summary>
    public static InjectionResult Success() => new(true, null);

    /// <summary>A failed injection.</summary>
    public static InjectionResult Failure(string errorMessage) => new(false, errorMessage);
}
