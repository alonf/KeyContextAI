namespace KeyContextAI.Core.Model;

/// <summary>
/// What a keystroke means to word assembly. Drives the word-completion rule in FR-005b: a word
/// completes on a separator or a committing key, and never mid-word.
/// </summary>
public enum KeyEventKind
{
    /// <summary>A character that becomes part of the word being typed.</summary>
    Character,

    /// <summary>Whitespace or punctuation that ends the current word without submitting input.</summary>
    Separator,

    /// <summary>A key such as Enter or Tab that ends the word and may submit the input to the application.</summary>
    Committing,

    /// <summary>Backspace or delete, which shortens the word in progress.</summary>
    Editing,

    /// <summary>A modifier key, which never contributes a character.</summary>
    Modifier,

    /// <summary>Anything else — navigation, function keys — which resets the word in progress.</summary>
    Other,
}
