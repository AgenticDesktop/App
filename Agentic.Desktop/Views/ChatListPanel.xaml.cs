using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.Desktop.ViewModels;
using Agentic.Desktop.ViewModels.Messages;

namespace Agentic.Desktop.Views;

public sealed partial class ChatListPanel : UserControl
{
    private ChatListViewModel? _viewModel;
    public ChatListViewModel? ViewModel
    {
        get => _viewModel;
        set { _viewModel = value; Bindings.Update(); }
    }

    public ChatListPanel()
    {
        InitializeComponent();
    }

    private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel?.SelectChatCommand.CanExecute(ChatListView.SelectedItem) == true
            && ChatListView.SelectedItem is ChatSession session)
        {
            ViewModel.SelectChatCommand.Execute(session);
        }
    }

    private void DeleteChat_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ChatSession session
            && ViewModel?.DeleteChatCommand.CanExecute(session) == true)
        {
            ViewModel.DeleteChatCommand.Execute(session);
        }
    }
}
