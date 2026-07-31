using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Agentic.ACPLibrary.Client;
using Agentic.ACPLibrary.Models;
using Agentic.Desktop.Services;
using Agentic.Desktop.ViewModels.Messages;

namespace Agentic.Desktop.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private IAcpClient? _acpClient;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
    private static readonly ObservableCollection<ChatMessage> _emptyMessages = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isAgentResponding;

    [ObservableProperty]
    private bool _isAgentConnected;

    [ObservableProperty]
    private ChatMessage? _currentAgentMessage;

    public ChatListViewModel ChatList { get; } = new();

    public ObservableCollection<ChatMessage> Messages =>
        ChatList.SelectedSession?.Messages ?? _emptyMessages;

    public event Action? ScrollToBottom;

    // Frame-level merging
    private readonly object _lock = new();
    private string _pendingText = "";
    private bool _flushScheduled;

    public ChatViewModel()
    {
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        ChatList.SessionChanged += OnSessionChanged;
    }

    private ObservableCollection<ChatMessage>? _subscribedMessages;
    private System.Collections.Specialized.NotifyCollectionChangedEventHandler? _messagesHandler;

    private void OnSessionChanged(ChatSession session)
    {
        // Cancel any in-progress streaming to prevent corruption
        if (IsAgentResponding)
        {
            _ = CancelGenerationAsync();
        }

        // Unsubscribe from old session's Messages collection
        if (_subscribedMessages is not null && _messagesHandler is not null)
        {
            _subscribedMessages.CollectionChanged -= _messagesHandler;
        }

        OnPropertyChanged(nameof(Messages));

        // Subscribe to new session's Messages collection
        _messagesHandler = (_, _) => ScrollToBottom?.Invoke();
        _subscribedMessages = Messages;
        _subscribedMessages.CollectionChanged += _messagesHandler;

        ScrollToBottom?.Invoke();
    }

    /// <summary>Binds an AcpClient (called after connection from Settings).</summary>
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

    /// <summary>Clears messages for the current session (called on disconnect).</summary>
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

        // Add user message
        Messages.Add(new ChatMessage(MessageRole.User, text));

        // Update session title and preview
        if (ChatList.SelectedSession is { } session)
        {
            if (session.Title == LocalizationService.Get("NewChatTitle"))
                session.Title = text.Length > 30 ? text[..30] + "..." : text;
            session.PreviewText = text.Length > 50 ? text[..50] + "..." : text;
            session.UpdatedAt = DateTime.Now;
        }

        // Create Agent message placeholder
        var agentMsg = new ChatMessage(MessageRole.Agent) { IsStreaming = true };
        Messages.Add(agentMsg);
        CurrentAgentMessage = agentMsg;
        IsAgentResponding = true;

        try
        {
            if (_acpClient is not null && _acpClient.CurrentSessionId is not null)
            {
                // Real ACP flow (or Mock transport via AcpClient)
                var prompt = new List<ContentBlock> { new TextContent { Text = text } };
                await _acpClient.SendPromptAsync(_acpClient.CurrentSessionId, prompt);
            }
            else
            {
                // Local Mock simulation when no AcpClient is available
                await SimulateMockResponseAsync(agentMsg, text);
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled - already handled in finally
        }
        catch (Exception ex)
        {
            agentMsg.TextContent += LocalizationService.Format("ErrorPrefix", ex.Message);
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
        // Frame-level merging: accumulate text, batch update UI
        switch (update)
        {
            case AgentMessageChunk chunk when chunk.Content is TextContent tc:
                lock (_lock)
                {
                    _pendingText += tc.Text;
                    if (!_flushScheduled)
                    {
                        _flushScheduled = true;
                        // Batch flush after 50ms
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
                    var currentMessages = ChatList.SelectedSession?.Messages;
                    if (currentMessages is not null)
                    {
                        var toolMsg = new ChatMessage(MessageRole.System,
                            LocalizationService.Format("ToolCallPrefix", toolCall.Title));
                        currentMessages.Add(toolMsg);
                    }

                    // Update session preview
                    if (ChatList.SelectedSession is { } session)
                    {
                        session.PreviewText = LocalizationService.Format("ToolCallPrefix", toolCall.Title);
                        session.UpdatedAt = DateTime.Now;
                    }
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

    /// <summary>Mock simulation method (used for UI testing without a real agent).</summary>
    private async Task SimulateMockResponseAsync(ChatMessage agentMsg, string userText)
    {
        var responses = new[]
        {
            LocalizationService.Get("MockResponse1"),
            $"\"{userText}\". ",
            LocalizationService.Get("MockResponse2"),
            LocalizationService.Get("MockResponse3")
        };
        foreach (var part in responses)
        {
            await Task.Delay(100);
            agentMsg.TextContent += part;
        }
    }
}
