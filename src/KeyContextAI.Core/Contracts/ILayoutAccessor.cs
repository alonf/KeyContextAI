using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Contracts;

/// <summary>
/// Reads and changes the keyboard layout used by the foreground application.
/// </summary>
public interface ILayoutAccessor
{
    /// <summary>Gets the layout active for the foreground window.</summary>
    LayoutId GetActiveLayout();

    /// <summary>Lists the installed keyboard layouts in stable native order.</summary>
    IReadOnlyList<LayoutId> GetInstalledLayouts();

    /// <summary>Requests the foreground application to activate an installed layout.</summary>
    bool TrySwitchLayout(LayoutId layout);
}
