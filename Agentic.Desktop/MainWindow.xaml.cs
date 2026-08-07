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
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
#else
        // Uno: Microsoft.UI.Xaml.Controls.TitleBar is not implemented. Show the
        // fallback Grid title bar and let NavigationView provide its own pane
        // toggle button (the TitleBar-based toggle is unavailable here).
        // Extend the fallback into the system title bar area to avoid a
        // double title bar (native Win32 caption + fallback Grid).
        AppTitleBar.Visibility = Visibility.Collapsed;
        AppTitleBarFallback.Visibility = Visibility.Visible;
        RootNavView.IsPaneToggleButtonVisible = true;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBarFallback);
#endif

        AppWindow.SetIcon("Assets/AppIcon.ico");

        // Set appropriate window size (object initializer: Uno projection lacks the 2-arg ctor)
        AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1200, Height = 780 });
    }

    /// <summary>Updates the connection status indicator.</summary>
    public void UpdateConnectionStatus(int state, string? agentName = null)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
#if WINDOWS
            var dot = StatusDot;
            var text = StatusText;
#else
            var dot = StatusDotFallback;
            var text = StatusTextFallback;
#endif
            switch (state)
            {
                case 0: // Disconnected
                    dot.Fill = new SolidColorBrush(Colors.Gray);
                    text.Text = LocalizationService.Get("StatusDisconnected");
                    break;
                case 1: // Connecting
                    dot.Fill = new SolidColorBrush(Colors.Gold);
                    text.Text = LocalizationService.Get("StatusConnecting");
                    break;
                case 2: // Connected
                    dot.Fill = new SolidColorBrush(Colors.LimeGreen);
                    text.Text = agentName ?? LocalizationService.Get("StatusConnected");
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
