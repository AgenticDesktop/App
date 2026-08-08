using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Agentic.Desktop.ViewModels;

namespace Agentic_Desktop;

/// <summary>
/// Browses the ACP registry and launches locally installed agents.
/// </summary>
public sealed partial class RegistryPage : Page
{
    public RegistryViewModel ViewModel { get; } = new();

    public RegistryPage()
    {
        InitializeComponent();

        // Agents launched from this page go through the shared SettingsViewModel connection flow;
        // ensure its connection callbacks are wired regardless of navigation order.
        App.RegisterConnectionHandlers(SettingsViewModel.Shared);

        // Auto-load on first visit
        Loaded += async (_, _) =>
        {
            if (ViewModel.Agents.Count == 0)
            {
                await ViewModel.RefreshCommand.ExecuteAsync(null);
            }
        };
    }

    /// <summary>
    /// When a remote agent icon fails to load, hide the <see cref="Image"/> and reveal the
    /// sibling initials <see cref="TextBlock"/> that lives in the same parent <see cref="Grid"/>.
    /// </summary>
    private void AgentIcon_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is not Image img || img.Parent is not Grid grid)
            return;

        img.Visibility = Visibility.Collapsed;

        // The initials TextBlock is the second child of the Grid.
        if (grid.Children.Count > 1 && grid.Children[1] is TextBlock tb)
        {
            tb.Visibility = Visibility.Visible;
        }
    }
}
