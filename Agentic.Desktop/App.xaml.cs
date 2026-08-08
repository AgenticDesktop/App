using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Agentic.ACPLibrary.Client;
using Agentic.Desktop.Services;
using Agentic.Desktop.ViewModels;
using Agentic.Desktop.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Agentic_Desktop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The main application window. Use <c>App.Window</c> from any class that needs
    /// the window reference (for dialogs, pickers, interop, etc.).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    /// The UI thread dispatcher. Use <c>App.DispatcherQueue</c> to marshal calls
    /// to the UI thread. Fully qualified to avoid CS0104 ambiguity with
    /// <see cref="Windows.System.DispatcherQueue"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    /// <summary>
    /// The native window handle (HWND). Use for file pickers,
    /// <c>DataTransferManager</c>, and any WinRT interop that requires
    /// <c>InitializeWithWindow</c>.
    /// </summary>
    public static nint WindowHandle
    {
        get
        {
#if WINDOWS
            return WinRT.Interop.WindowNative.GetWindowHandle(Window);
#else
            return nint.Zero;
#endif
        }
    }

    /// <summary>
    /// Currently connected AcpClient (set by Settings page, read by Chat page)
    /// </summary>
    public static IAcpClient? CurrentAcpClient { get; set; }

    /// <summary>Raised when connection state changes.</summary>
    public static event Action<IAcpClient?>? AcpClientChanged;

    /// <summary>Global logger factory.</summary>
    public static ILoggerFactory LoggerFactory { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Configure logging
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }

    /// <summary>Sets the current AcpClient and notifies subscribers.</summary>
    public static void SetAcpClient(IAcpClient? client)
    {
        CurrentAcpClient = client;
        AcpClientChanged?.Invoke(client);
    }

    /// <summary>
    /// Attaches UI handlers (permission, file system) to a freshly connected AcpClient
    /// and publishes it as the current client. Shared by the Settings page and the Registry page.
    /// </summary>
    public static void AttachAgentClient(IAcpClient client, string workingDirectory)
    {
        var permHandler = new DesktopPermissionHandler(DispatcherQueue);
        permHandler.PermissionRequested += async args =>
        {
            var dialog = new PermissionDialog(args.Request)
            {
                XamlRoot = Window.Content.XamlRoot
            };
            await dialog.ShowAsync();
            var result = await dialog.Result;
            args.OnComplete(result);
        };
        client.PermissionHandler = permHandler;
        client.FileSystemHandler = new DesktopFileSystemHandler(workingDirectory);
        SetAcpClient(client);
    }

    /// <summary>
    /// Wires the shared connection callbacks (agent connected / disconnected) onto the settings ViewModel.
    /// Idempotent: called by every page that may trigger a connection, so the callbacks are complete
    /// regardless of navigation order.
    /// </summary>
    public static void RegisterConnectionHandlers(SettingsViewModel vm)
    {
        vm.OnAgentConnected = client =>
        {
            AttachAgentClient(client, vm.WorkingDirectory);
            UpdateWindowStatus(vm);
        };
        vm.OnAgentDisconnected = message =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                vm.ConnectionStatus = message;
                vm.IsConnected = false;
                vm.ConnectionState = 0;
                SetAcpClient(null);
            });
        };
    }

    /// <summary>Refreshes the title-bar connection indicator from the shared settings ViewModel.</summary>
    public static void UpdateWindowStatus(SettingsViewModel vm)
    {
        if (Window is MainWindow mainWindow)
        {
            mainWindow.UpdateConnectionStatus(vm.ConnectionState, vm.ConnectionState == 2 ? vm.AgentName : null);
        }
    }
}
