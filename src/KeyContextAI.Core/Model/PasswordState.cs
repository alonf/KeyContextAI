namespace KeyContextAI.Core.Model;

/// <summary>
/// Whether the focused control is a password context.
/// </summary>
public enum PasswordState
{
    /// <summary>The accessor could not determine the password state.</summary>
    Unknown = 0,

    /// <summary>The focused control is not a password field.</summary>
    No = 1,

    /// <summary>The focused control is a password field.</summary>
    Yes = 2,
}
