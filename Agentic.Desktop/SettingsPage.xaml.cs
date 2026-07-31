using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.ACPLibrary.Client;
using Agentic.Desktop.Services;
using Agentic.Desktop.ViewModels;
using Agentic.Desktop.Views;

namespace Agentic_Desktop;

public sealed partial class SettingsPage : Page
{
    // Use global shared ViewModel: Frame.Navigate recreates the page each time, connection state must not be lost with the page
    public SettingsViewModel ViewModel { get; } = SettingsViewModel.Shared;

    public SettingsPage()
    {
        InitializeComponent();

        // After connection, store AcpClient in App and notify ChatViewModel
        ViewModel.OnAgentConnected = client =>
        {
            // Create permission handler and subscribe to events
            var permHandler = new DesktopPermissionHandler(App.DispatcherQueue);
            permHandler.PermissionRequested += async args =>
            {
                var dialog = new PermissionDialog(args.Request)
                {
                    XamlRoot = App.Window.Content.XamlRoot
                };
                await dialog.ShowAsync();
                var result = await dialog.Result;
                args.OnComplete(result);
            };
            client.PermissionHandler = permHandler;

            // Create file system handler
            client.FileSystemHandler = new DesktopFileSystemHandler(ViewModel.WorkingDirectory);

            // Update title bar connection status
            UpdateConnectionStatus();

            App.SetAcpClient(client);
        };

        // Update UI when Agent disconnects
        ViewModel.OnAgentDisconnected = message =>
        {
            App.DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.ConnectionStatus = message;
                ViewModel.IsConnected = false;
                ViewModel.ConnectionState = 0;
                App.SetAcpClient(null);
            });
        };
        // Listen for connection state changes (shared VM outlives the page; unsubscribe on unload to avoid duplicate subscriptions)
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += (_, _) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.ConnectionState))
        {
            UpdateConnectionStatus();
        }
    }

    private void UpdateConnectionStatus()
    {
        if (App.Window is MainWindow mainWindow)
        {
            mainWindow.UpdateConnectionStatus(
                ViewModel.ConnectionState,
                ViewModel.ConnectionState == 2 ? ViewModel.AgentName : null);
        }
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
        picker.FileTypeFilter.Add("*");

        // WinUI 3: need to initialize with hwnd
        var hwnd = App.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            ViewModel.WorkingDirectory = folder.Path;
        }
    }
}
