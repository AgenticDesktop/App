using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Agentic.Desktop.ViewModels.Messages;

/// <summary>
/// Represents a chat conversation session with its own message history.
/// </summary>
public partial class ChatSession : ObservableObject
{
    public string Id { get; } = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string _title = "New Chat";

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.Now;

    [ObservableProperty]
    private string _previewText = "";

    public ObservableCollection<ChatMessage> Messages { get; } = new();
}
