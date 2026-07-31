using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Agentic.ACPLibrary.Client;
using Agentic.ACPLibrary.Models;
using Agentic.Desktop.ViewModels.Messages;

namespace Agentic.Desktop.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private IAcpClient? _acpClient;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isAgentResponding;

    [ObservableProperty]
    private bool _isAgentConnected;

    [ObservableProperty]
    private ChatMessage? _currentAgentMessage;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    // 帧级合并相关
    private readonly object _lock = new();
    private string _pendingText = "";
    private bool _flushScheduled;

    public ChatViewModel()
    {
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    /// <summary>绑定 AcpClient（从 Settings 连接后调用）</summary>
    public void BindClient(IAcpClient client)
    {
        if (_acpClient is not null)
        {
            _acpClient.SessionUpdated -= OnSessionUpdated;
        }

        _acpClient = client;
        _acpClient.SessionUpdated += OnSessionUpdated;
        IsAgentConnected = true;
    }

    /// <summary>清除所有消息（断开连接时调用）</summary>
    public void ClearMessages()
    {
        Messages.Clear();
        IsAgentConnected = false;
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsAgentResponding)
            return;

        var text = InputText;
        InputText = "";

        // 添加用户消息
        Messages.Add(new ChatMessage(MessageRole.User, text));

        // 创建 Agent 消息占位
        var agentMsg = new ChatMessage(MessageRole.Agent) { IsStreaming = true };
        Messages.Add(agentMsg);
        CurrentAgentMessage = agentMsg;
        IsAgentResponding = true;

        try
        {
            if (_acpClient is not null && _acpClient.CurrentSessionId is not null)
            {
                // 真实 ACP 流程（或 Mock transport 通过 AcpClient）
                var prompt = new List<ContentBlock> { new TextContent { Text = text } };
                await _acpClient.SendPromptAsync(_acpClient.CurrentSessionId, prompt);
            }
            else
            {
                // 无 AcpClient 时的本地 Mock 模拟
                await SimulateMockResponseAsync(agentMsg, text);
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled - already handled in finally
        }
        catch (Exception ex)
        {
            agentMsg.TextContent += $"\n[Error: {ex.Message}]";
        }
        finally
        {
            agentMsg.IsStreaming = false;
            CurrentAgentMessage = null;
            IsAgentResponding = false;
        }
    }

    private Task OnSessionUpdated(SessionUpdate update)
    {
        // 帧级合并：累积文本，批量更新 UI
        switch (update)
        {
            case AgentMessageChunk chunk when chunk.Content is TextContent tc:
                lock (_lock)
                {
                    _pendingText += tc.Text;
                    if (!_flushScheduled)
                    {
                        _flushScheduled = true;
                        // 50ms 后批量刷新
                        _ = Task.Delay(50).ContinueWith(_ =>
                        {
                            lock (_lock)
                            {
                                var batchText = _pendingText;
                                _pendingText = "";
                                _flushScheduled = false;
                                _dispatcherQueue.TryEnqueue(() =>
                                {
                                    if (CurrentAgentMessage is not null)
                                        CurrentAgentMessage.TextContent += batchText;
                                });
                            }
                        });
                    }
                }
                break;

            case ToolCallNotification toolCall:
                _dispatcherQueue.TryEnqueue(() =>
                {
                    var toolMsg = new ChatMessage(MessageRole.System,
                        $"[Tool: {toolCall.Title}]");
                    Messages.Add(toolMsg);
                });
                break;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CancelGenerationAsync()
    {
        if (_acpClient?.CurrentSessionId is not null)
        {
            await _acpClient.CancelSessionAsync(_acpClient.CurrentSessionId);
        }
        IsAgentResponding = false;
        if (CurrentAgentMessage is not null)
            CurrentAgentMessage.IsStreaming = false;
    }

    /// <summary>Mock 模拟方法（用于无真实 agent 时的 UI 测试）</summary>
    private async Task SimulateMockResponseAsync(ChatMessage agentMsg, string userText)
    {
        var responses = new[]
        {
            "I received your message: ",
            $"\"{userText}\". ",
            "I'm a mock agent running in the UI. ",
            "Connect a real ACP agent in Settings to get actual responses."
        };
        foreach (var part in responses)
        {
            await Task.Delay(100);
            agentMsg.TextContent += part;
        }
    }
}
