namespace KeyContextAI.Core.Model;

/// <summary>
/// The modifier keys that were held when a key was pressed.
/// </summary>
/// <remarks>
/// Captured with the keystroke so a suppressed key can be re-delivered as the gesture the user
/// actually made. A committing key is suppressed while the correction runs, and the user is free to
/// release the modifier in that window; replaying the bare key would turn Shift+Enter into Enter,
/// which in a chat client sends a message the user meant to break a line in.
/// </remarks>
[Flags]
public enum KeyModifiers
{
    /// <summary>No modifier was held.</summary>
    None = 0,

    /// <summary>Either Shift key was held.</summary>
    Shift = 1,

    /// <summary>Either Control key was held.</summary>
    Control = 2,

    /// <summary>Either Alt key was held.</summary>
    Alt = 4,

    /// <summary>Either Windows key was held.</summary>
    Windows = 8,
}
