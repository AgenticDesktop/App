using Agentic.Desktop.ViewModels.Messages;
using Xunit;

namespace Agentic.Desktop.Tests;

public class ChatMessageTests
{
    [Fact]
    public void Constructor_SetsRoleAndText()
    {
        var msg = new ChatMessage(MessageRole.User, "Hello");

        Assert.Equal(MessageRole.User, msg.Role);
        Assert.Equal("Hello", msg.TextContent);
    }

    [Fact]
    public void Constructor_DefaultText_IsEmpty()
    {
        var msg = new ChatMessage(MessageRole.Agent);

        Assert.Equal(string.Empty, msg.TextContent);
    }

    [Fact]
    public void Id_IsUniquePerInstance()
    {
        var msg1 = new ChatMessage(MessageRole.User);
        var msg2 = new ChatMessage(MessageRole.User);

        Assert.NotEqual(msg1.Id, msg2.Id);
    }

    [Fact]
    public void Id_IsNotEmpty()
    {
        var msg = new ChatMessage(MessageRole.System);

        Assert.False(string.IsNullOrWhiteSpace(msg.Id));
    }

    [Fact]
    public void Timestamp_IsSetToRecentTime()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var msg = new ChatMessage(MessageRole.User);
        var after = DateTime.Now.AddSeconds(1);

        Assert.InRange(msg.Timestamp, before, after);
    }

    [Fact]
    public void TextContent_RaisesPropertyChanged()
    {
        var msg = new ChatMessage(MessageRole.Agent);
        var raised = false;
        msg.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ChatMessage.TextContent))
                raised = true;
        };

        msg.TextContent = "new text";

        Assert.True(raised);
    }

    [Fact]
    public void IsStreaming_DefaultsToFalse()
    {
        var msg = new ChatMessage(MessageRole.Agent);

        Assert.False(msg.IsStreaming);
    }

    [Fact]
    public void IsStreaming_RaisesPropertyChanged()
    {
        var msg = new ChatMessage(MessageRole.Agent);
        var raised = false;
        msg.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ChatMessage.IsStreaming))
                raised = true;
        };

        msg.IsStreaming = true;

        Assert.True(raised);
    }

    [Fact]
    public void TextContent_AppendAccumulates()
    {
        var msg = new ChatMessage(MessageRole.Agent);
        msg.TextContent += "Hello ";
        msg.TextContent += "World";

        Assert.Equal("Hello World", msg.TextContent);
    }
}
