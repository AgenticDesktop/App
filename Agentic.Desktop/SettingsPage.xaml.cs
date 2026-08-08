using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.Desktop.ViewModels;

namespace Agentic_Desktop;

public sealed partial class SettingsPage : Page
{
    // Use global shared ViewModel: Frame.Navigate recreates the page each time, connection state must not be lost with the page
    public SettingsViewModel ViewModel { get; } = SettingsViewModel.Shared;

    public SettingsPage()
    {
        InitializeComponent();

        // Shared connection handlers: attach UI handlers, publish AcpClient, update title bar
        App.RegisterConnectionHandlers(ViewModel);

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

#if WINDOWS
        // WinUI 3: need to initialize with hwnd
        var hwnd = App.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            ViewModel.WorkingDirectory = folder.Path;
        }
    }
}
