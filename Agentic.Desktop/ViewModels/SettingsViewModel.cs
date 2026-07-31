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
    /// <summary>全局共享实例：页面每次导航都会重建，连接状态必须跨页面实例保持</summary>
    public static SettingsViewModel Shared { get; } = new();

    [ObservableProperty]
    private string _agentPath = string.Empty;

    [ObservableProperty]
    private string _agentArguments = string.Empty;

    [ObservableProperty]
    private string _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    [ObservableProperty]
    private string _agentName = string.Empty;

    [ObservableProperty]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private bool _isConnected;

    /// <summary>连接状态枚举: 0=Disconnected, 1=Connecting, 2=Connected</summary>
    [ObservableProperty]
    private int _connectionState;

    public IAcpClient? AcpClient { get; private set; }

    private TerminalManager? _terminalManager;

    private Func<int, Task>? _agentProcessExitedHandler;

    /// <summary>连接成功后通知外部（MainPage）传递 AcpClient</summary>
    public Action<IAcpClient>? OnAgentConnected { get; set; }

    /// <summary>Agent 进程意外断开时触发</summary>
    public Action<string>? OnAgentDisconnected { get; set; }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (IsConnecting) return;
        IsConnecting = true;
        ConnectionState = 1; // Connecting
        ConnectionStatus = "连接中...";

        try
        {
            // 如果已有连接，先断开
            await CleanupAsync();

            IAgentTransport transport;

            if (string.IsNullOrWhiteSpace(AgentPath))
            {
                // 使用 Mock 传输
                transport = new MockAgentTransport();
            }
            else
            {
                transport = new StdioAgentTransport(AgentPath, AgentArguments, WorkingDirectory);
            }

            var dispatcher = new JsonRpcDispatcher();
            var logger = App.LoggerFactory?.CreateLogger<AcpClient>();
            AcpClient = new AcpClient(transport, dispatcher, logger);

            // 订阅 Agent 进程退出事件
            _agentProcessExitedHandler = exitCode =>
            {
                OnAgentDisconnected?.Invoke($"Agent 已断开 (exit code: {exitCode})");
                return Task.CompletedTask;
            };
            AcpClient.AgentProcessExited += _agentProcessExitedHandler;

            var info = await AcpClient.InitializeAsync();
            AgentName = info.AgentInfo?.Title ?? info.AgentInfo?.Name ?? "Unknown Agent";

            // 创建 TerminalManager
            _terminalManager = new TerminalManager();
            AcpClient.TerminalHandler = _terminalManager;

            // 创建会话
            var sid = await AcpClient.CreateSessionAsync(WorkingDirectory);
            SessionId = sid;

            ConnectionStatus = "已连接";
            IsConnected = true;
            ConnectionState = 2; // Connected

            // 通知 ChatViewModel 使用新的 AcpClient
            OnAgentConnected?.Invoke(AcpClient);
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"连接失败: {ex.Message}";
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
        ConnectionStatus = "未连接";
        AgentName = string.Empty;
        SessionId = string.Empty;
        IsConnected = false;
        ConnectionState = 0; // Disconnected

        // 同步全局状态，通知 ChatViewModel 等订阅者
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
