using System.Windows;

namespace KeyContextAI.App;

/// <summary>
/// The application's placeholder window.
/// </summary>
/// <remarks>
/// KeyContext AI is tray-resident and has no main window in its finished form — the user-facing
/// surfaces are the tray menu, the correction bubble, and a settings window opened on demand, all of
/// which arrive with the client tasks in a later iteration. This window exists only so the WPF host
/// has an entry point while iteration 001 builds the engines.
/// </remarks>
public partial class MainWindow : Window
{
    /// <summary>Creates the placeholder window.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }
}
