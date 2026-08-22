namespace KeyContextAI.Core.Model;

/// <summary>
/// An opaque marker for one armed suppression transaction.
/// </summary>
public readonly record struct SuppressionToken(Guid Value)
{
    /// <summary>Creates a new token.</summary>
    public static SuppressionToken Create() => new(Guid.NewGuid());
}
