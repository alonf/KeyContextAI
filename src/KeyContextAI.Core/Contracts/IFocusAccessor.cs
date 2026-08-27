using System.Drawing;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Publishes foreground and control focus changes and reports focus metadata.
/// </summary>
/// <remarks>
/// A resource accessor: it touches the outside world and calls nothing inside the system. The
/// manager decides how to react to the published focus changes.
/// </remarks>
public interface IFocusAccessor
{
    /// <summary>Published whenever the foreground window or focused control changes.</summary>
    event Action<FocusContext> FocusChanged;

    /// <summary>Publishes a snapshot of whatever currently has focus.</summary>
    /// <remarks>
    /// Focus notifications describe <em>changes</em>. Without an explicit initial snapshot, an
    /// application started while the user is already typing has no focus context at all, so every
    /// key is rejected as unattributable and capture stays dead until the user happens to switch
    /// windows. Called once at startup, before capture is declared ready.
    /// </remarks>
    void PublishCurrentFocus();

    /// <summary>Determines whether the current focused control is a password field.</summary>
    /// <returns><see cref="PasswordState.Yes"/>, <see cref="PasswordState.No"/>, or
    /// <see cref="PasswordState.Unknown"/>. Never throws.</returns>
    PasswordState IsPasswordContext();

    /// <summary>Attempts to read the current caret position for bubble placement.</summary>
    /// <param name="p">The caret position, in screen coordinates, when one is available.</param>
    /// <returns>True when a caret position could be read.</returns>
    bool TryGetCaretPosition(out Point p);
}
