using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.ACPLibrary.Client;
using Agentic.Desktop.ViewModels;
using Agentic.Desktop.ViewModels.Messages;

namespace Agentic_Desktop;

/// <summary>
/// The main content page displaying the chat UI.
/// </summary>
public sealed partial class MainPage : Page
{
    public ChatViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();
        ViewModel.Messages.CollectionChanged += (_, _) => ScrollToBottom();

        // 如果已有连接的 AcpClient，立即绑定
        if (App.CurrentAcpClient is not null)
        {
            ViewModel.BindClient(App.CurrentAcpClient);
        }

        // 订阅未来的连接变更
        App.AcpClientChanged += OnAcpClientChanged;
    }

    private void OnAcpClientChanged(IAcpClient? client)
    {
        if (client is not null)
        {
            ViewModel.BindClient(client);
        }
        else
        {
            // 断开连接时清除消息
            ViewModel.ClearMessages();
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        if (App.Window is MainWindow mainWindow)
        {
            mainWindow.NavigateToSettings();
        }
    }

    private void InputTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.WasKeyDown)
        {
            e.Handled = true;
            if (ViewModel.SendMessageCommand.CanExecute(null))
                ViewModel.SendMessageCommand.Execute(null);
        }
    }

    private void ScrollToBottom()
    {
        // ScrollViewer doesn't have a direct ScrollToEnd in WinUI 3,
        // so we scroll to the extent after layout.
        DispatcherQueue.TryEnqueue(() =>
        {
            MessageScroller.ChangeView(null, MessageScroller.ScrollableHeight, null);
        });
    }
}

/// <summary>
/// Selects a DataTemplate based on <see cref="ChatMessage.Role"/>.
/// </summary>
public partial class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }
    public DataTemplate? AgentTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is ChatMessage msg)
        {
            return msg.Role switch
            {
                MessageRole.User => UserTemplate,
                _ => AgentTemplate
            };
        }
        return AgentTemplate;
    }
}

