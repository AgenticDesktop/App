using CommunityToolkit.Mvvm.ComponentModel;

namespace Agentic.Desktop.ViewModels.Messages;

/// <summary>
/// Represents a single chat message in the conversation.
/// <para>
/// TextContent may contain Markdown from the Agent. WinUI 3 TextBlock
/// does not natively render HTML/Markdown, so the raw text is displayed.
/// Use <see cref="Agentic.Desktop.Services.MarkdownHelper"/> to convert
/// to HTML for future WebView2 rendering, or to plain text.
/// </para>
/// </summary>
public partial class ChatMessage : ObservableObject
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public MessageRole Role { get; }
    public DateTime Timestamp { get; } = DateTime.Now;

    [ObservableProperty]
    private string _textContent = string.Empty;

    [ObservableProperty]
    private bool _isStreaming;

    public ChatMessage(MessageRole role, string textContent = "")
    {
        Role = role;
        TextContent = textContent;
    }
}

public enum MessageRole
{
    User,
    Agent,
    System
}
