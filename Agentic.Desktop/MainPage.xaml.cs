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

    /// <summary>x:Bind helper: collapse a TextBlock when a string is null/empty.</summary>
    public static Visibility HasText(string? text) =>
        string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;

    private void InputTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // When the autocomplete popup is open, arrow keys / Enter / Tab drive selection
        // and Esc closes the popup without disturbing the current input.
        if (ViewModel.IsCommandPopupOpen)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Down:
                    e.Handled = true;
                    MoveCommandSelection(1);
                    return;
                case Windows.System.VirtualKey.Up:
                    e.Handled = true;
                    MoveCommandSelection(-1);
                    return;
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.Tab:
                    if (ViewModel.SelectedCommandIndex >= 0 &&
                        ViewModel.SelectedCommandIndex < ViewModel.FilteredCommands.Count)
                    {
                        e.Handled = true;
                        InsertCommand(ViewModel.FilteredCommands[ViewModel.SelectedCommandIndex]);
                    }
                    return;
                case Windows.System.VirtualKey.Escape:
                    e.Handled = true;
                    ViewModel.IsCommandPopupOpen = false;
                    return;
            }
        }

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

    private void MoveCommandSelection(int delta)
    {
        var count = ViewModel.FilteredCommands.Count;
        if (count == 0)
            return;

        var next = ViewModel.SelectedCommandIndex + delta;
        if (next < 0)
            next = 0;
        if (next >= count)
            next = count - 1;

        ViewModel.SelectedCommandIndex = next;
        if (CommandListView.ContainerFromIndex(next) is ListViewItem item)
            item.StartBringIntoView();
    }

    private void InsertCommand(Agentic.ACPLibrary.Models.AvailableCommand command)
    {
        var newText = ViewModel.ApplyCommand(command);
        InputTextBox.Text = newText;
        InputTextBox.SelectionStart = newText.Length;
        InputTextBox.SelectionLength = 0;
        InputTextBox.Focus(FocusState.Programmatic);
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.OnInputTextChanged();
    }

    private void CommandListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Agentic.ACPLibrary.Models.AvailableCommand cmd)
        {
            InsertCommand(cmd);
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

