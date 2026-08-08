using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Agentic.ACPLibrary.Client;
using Agentic.ACPLibrary.Models;
using Agentic.ACPLibrary.Protocol;
using Agentic.ACPLibrary.Transport;
using Agentic.Desktop.Mocks;
using Agentic.Desktop.Services;

using Agentic_Desktop;

namespace Agentic.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    /// <summary>Globally shared instance: pages are recreated on each navigation, connection state must persist across page instances.</summary>
    public static SettingsViewModel Shared { get; } = new();

    [ObservableProperty]
    private string _agentPath = string.Empty;

    [ObservableProperty]
    private string _agentArguments = string.Empty;

    [ObservableProperty]
    private string _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty]
    private string _connectionStatus = LocalizationService.Get("StatusNotConnected");

    [ObservableProperty]
    private string _agentName = string.Empty;

    [ObservableProperty]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private bool _isConnected;

    /// <summary>Connection state enum: 0=Disconnected, 1=Connecting, 2=Connected</summary>
    [ObservableProperty]
    private int _connectionState;

    public IAcpClient? AcpClient { get; private set; }

    private TerminalManager? _terminalManager;

    private Func<int, Task>? _agentProcessExitedHandler;

    /// <summary>Notifies external consumers (MainPage) after successful connection, passing the AcpClient.</summary>
    public Action<IAcpClient>? OnAgentConnected { get; set; }

    /// <summary>Raised when the Agent process disconnects unexpectedly.</summary>
    public Action<string>? OnAgentDisconnected { get; set; }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (IsConnecting) return;
        IsConnecting = true;
        ConnectionState = 1; // Connecting
        ConnectionStatus = LocalizationService.Get("StatusConnectingProgress");

        try
        {
            // Disconnect existing connection first
            await CleanupAsync();

            IAgentTransport transport;

            if (string.IsNullOrWhiteSpace(AgentPath))
            {
                // Use Mock transport
                transport = new MockAgentTransport();
            }
            else
            {
                // Resolve command name to full path (e.g., "npx" -> "C:\Program Files\nodejs\npx.cmd")
                var resolvedPath = CommandResolver.ResolveCommand(AgentPath);
                transport = new StdioAgentTransport(resolvedPath, AgentArguments, WorkingDirectory);
            }

            var dispatcher = new JsonRpcDispatcher();
            var logger = App.LoggerFactory?.CreateLogger<AcpClient>();
            AcpClient = new AcpClient(transport, dispatcher, logger);

            // Subscribe to Agent process exit event
            _agentProcessExitedHandler = exitCode =>
            {
                OnAgentDisconnected?.Invoke(LocalizationService.Format("StatusAgentDisconnected", exitCode));
                return Task.CompletedTask;
            };
            AcpClient.AgentProcessExited += _agentProcessExitedHandler;

            var info = await AcpClient.InitializeAsync();
            AgentName = info.AgentInfo?.Title ?? info.AgentInfo?.Name ?? "Unknown Agent";

            // Create TerminalManager
            _terminalManager = new TerminalManager();
            AcpClient.TerminalHandler = _terminalManager;

            // Create session
            var sid = await AcpClient.CreateSessionAsync(WorkingDirectory);
            SessionId = sid;

            ConnectionStatus = LocalizationService.Get("StatusConnectedConfirm");
            IsConnected = true;
            ConnectionState = 2; // Connected

            // Notify ChatViewModel to use the new AcpClient
            OnAgentConnected?.Invoke(AcpClient);
        }
        catch (Exception ex)
        {
            ConnectionStatus = LocalizationService.Format("StatusConnectionFailed", ex.Message);
            AgentName = string.Empty;
            SessionId = string.Empty;
            ConnectionState = 0; // Disconnected
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        await CleanupAsync();
        ConnectionStatus = LocalizationService.Get("StatusNotConnected");
        AgentName = string.Empty;
        SessionId = string.Empty;
        IsConnected = false;
        ConnectionState = 0; // Disconnected

        // Sync global state, notify ChatViewModel and other subscribers
        App.SetAcpClient(null);
    }

    private async Task CleanupAsync()
    {
        if (AcpClient is not null)
        {
            if (_agentProcessExitedHandler is not null)
            {
                AcpClient.AgentProcessExited -= _agentProcessExitedHandler;
                _agentProcessExitedHandler = null;
            }
            await AcpClient.ShutdownAsync();
            AcpClient = null;
        }

        if (_terminalManager is not null)
        {
            _terminalManager.Dispose();
            _terminalManager = null;
        }
    }
}
