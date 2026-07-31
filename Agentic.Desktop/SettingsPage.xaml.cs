using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.ACPLibrary.Client;
using Agentic.Desktop.Services;
using Agentic.Desktop.ViewModels;
using Agentic.Desktop.Views;

namespace Agentic_Desktop;

public sealed partial class SettingsPage : Page
{
    // 使用全局共享 ViewModel：Frame.Navigate 每次重建页面，连接状态不能随页面丢失
    public SettingsViewModel ViewModel { get; } = SettingsViewModel.Shared;

    public SettingsPage()
    {
        InitializeComponent();

        // 连接成功后将 AcpClient 存储到 App 并通知 ChatViewModel
        ViewModel.OnAgentConnected = client =>
        {
            // 创建权限处理器并订阅事件
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

            // 创建文件系统处理器
            client.FileSystemHandler = new DesktopFileSystemHandler(ViewModel.WorkingDirectory);

            // 更新标题栏连接状态
            UpdateConnectionStatus();

            App.SetAcpClient(client);
        };

        // Agent 断开时更新 UI
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
        // 监听连接状态变化（共享 VM 生命周期长于页面，卸载时退订避免重复订阅）
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
