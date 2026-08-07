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

        // Bind ChatList to sidebar
        ChatListPanel.ViewModel = ViewModel.ChatList;

        // Subscribe to scroll event
        ViewModel.ScrollToBottom += ScrollToBottom;

        // If a connected AcpClient already exists, bind immediately
        if (App.CurrentAcpClient is not null)
        {
            ViewModel.BindClient(App.CurrentAcpClient);
        }

        // Subscribe to future connection changes
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
            // Clear messages on disconnect
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

    /// <summary>x:Bind helper: send is allowed only when connected and input is non-empty.</summary>
    public static bool CanSend(bool isConnected, string text) =>
        isConnected && !string.IsNullOrWhiteSpace(text);

    private void InputTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.WasKeyDown)
        {
            // Multiline input: Shift+Enter inserts a newline, Enter alone sends.
            var shiftDown = Microsoft.UI.Input.InputKeyboardSource
                .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            if (shiftDown)
                return;

            e.Handled = true;
            if (ViewModel.SendMessageCommand.CanExecute(null))
                ViewModel.SendMessageCommand.Execute(null);
        }
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        ChatSplitView.IsPaneOpen = !ChatSplitView.IsPaneOpen;
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

