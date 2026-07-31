using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Agentic.Desktop.ViewModels.Messages;

namespace Agentic.Desktop.ViewModels;

public partial class ChatListViewModel : ObservableObject
{
    public ObservableCollection<ChatSession> Sessions { get; } = new();

    [ObservableProperty]
    private ChatSession? _selectedSession;

    public event Action<ChatSession>? SessionChanged;

    public ChatListViewModel()
    {
        CreateNewChat();
    }

    [RelayCommand]
    private void CreateNewChat()
    {
        var session = new ChatSession();
        Sessions.Insert(0, session);
        SelectChat(session);
    }

    [RelayCommand]
    private void DeleteChat(ChatSession session)
    {
        if (session is null) return;

        var index = Sessions.IndexOf(session);
        if (index < 0) return;

        Sessions.RemoveAt(index);

        if (SelectedSession == session)
        {
            if (Sessions.Count > 0)
            {
                // 选择相邻的 session
                var newIndex = Math.Min(index, Sessions.Count - 1);
                SelectChat(Sessions[newIndex]);
            }
            else
            {
                // 列表为空，自动创建新 session
                CreateNewChat();
            }
        }
    }

    [RelayCommand]
    private void SelectChat(ChatSession session)
    {
        if (session is null) return;
        SelectedSession = session;
        SessionChanged?.Invoke(session);
    }
}
