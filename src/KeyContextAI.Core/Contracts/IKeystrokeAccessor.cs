using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Captures keystrokes from the active desktop.
/// </summary>
/// <remarks>
/// A resource accessor: it touches the outside world and calls nothing inside the system. The hook
/// callback must stay allocation-free and O(1), because it sits on the user's typing path.
/// </remarks>
public interface IKeystrokeAccessor
{
    /// <summary>Published for every non-self-injected keystroke.</summary>
    event Action<KeyEvent> KeyObserved;

    /// <summary>Arms the hook to suppress the next committing keystroke.</summary>
    void Arm(SuppressionToken token);

    /// <summary>Disarms the hook.</summary>
    void Disarm();

    /// <summary>Installs the low-level keyboard hook.</summary>
    /// <returns>A task that completes when the hook is ready.</returns>
    Task InstallAsync();

    /// <summary>Uninstalls the low-level keyboard hook.</summary>
    /// <returns>A task that completes when the hook is down.</returns>
    Task UninstallAsync();
}
