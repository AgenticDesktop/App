using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using Agentic.Desktop.Services;

namespace Agentic_Desktop;

/// <summary>
/// The application window. This hosts a NavigationView that allows switching
/// between the Chat page and the Settings page.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if WINDOWS
        SystemBackdrop = new MicaBackdrop();
#endif

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        // Set appropriate window size (object initializer: Uno projection lacks the 2-arg ctor)
        AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1200, Height = 780 });
    }

    /// <summary>Updates the connection status indicator.</summary>
    public void UpdateConnectionStatus(int state, string? agentName = null)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (state)
            {
                case 0: // Disconnected
                    StatusDot.Fill = new SolidColorBrush(Colors.Gray);
                    StatusText.Text = LocalizationService.Get("StatusDisconnected");
                    break;
                case 1: // Connecting
                    StatusDot.Fill = new SolidColorBrush(Colors.Gold);
                    StatusText.Text = LocalizationService.Get("StatusConnecting");
                    break;
                case 2: // Connected
                    StatusDot.Fill = new SolidColorBrush(Colors.LimeGreen);
                    StatusText.Text = agentName ?? LocalizationService.Get("StatusConnected");
                    break;
            }
        });
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (RootFrame.CanGoBack)
        {
            RootFrame.GoBack();
        }
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        RootNavView.IsPaneOpen = !RootNavView.IsPaneOpen;
    }

    private void RootNavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Default to Chat page
        RootNavView.SelectedItem = RootNavView.MenuItems[0];
        RootFrame.Navigate(typeof(MainPage));
    }

    private void RootNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            switch (tag)
            {
                case "chat":
                    RootFrame.Navigate(typeof(MainPage));
                    break;
                case "settings":
                    RootFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }
    }

    /// <summary>Navigates to the settings page.</summary>
    public void NavigateToSettings()
    {
        RootNavView.SelectedItem = RootNavView.FooterMenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag?.ToString() == "settings");
    }
}
