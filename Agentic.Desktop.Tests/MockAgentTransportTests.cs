using System.Text.Json;
using Agentic.Desktop.Mocks;
using Agentic.ACPLibrary.Transport;
using Xunit;

namespace Agentic.Desktop.Tests;

public class MockAgentTransportTests
{
    private readonly MockAgentTransport _transport = new();

    [Fact]
    public void InitialState_IsCreated()
    {
        Assert.Equal(TransportState.Created, _transport.State);
    }

    [Fact]
    public async Task StartAsync_SetsStateToRunning()
    {
        await _transport.StartAsync();

        Assert.Equal(TransportState.Running, _transport.State);
    }

    [Fact]
    public async Task StopAsync_SetsStateToStopped()
    {
        await _transport.StartAsync();
        await _transport.StopAsync();

        Assert.Equal(TransportState.Stopped, _transport.State);
    }

    [Fact]
    public async Task SendAsync_WhenNotRunning_DoesNotFireMessage()
    {
        var fired = false;
        _transport.MessageReceived += _ => { fired = true; return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"initialize","id":1}""");

        Assert.False(fired);
    }

    [Fact]
    public async Task SendAsync_Initialize_ReturnsAgentInfo()
    {
        await _transport.StartAsync();
        string? received = null;
        _transport.MessageReceived += msg => { received = msg; return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"initialize","id":1}""");

        Assert.NotNull(received);
        using var doc = JsonDocument.Parse(received);
        var root = doc.RootElement;
        Assert.Equal(1, root.GetProperty("id").GetInt32());
        Assert.Equal("mock-agent", root.GetProperty("result").GetProperty("agentInfo").GetProperty("name").GetString());
    }

    [Fact]
    public async Task SendAsync_SessionNew_ReturnsSessionId()
    {
        await _transport.StartAsync();
        string? received = null;
        _transport.MessageReceived += msg => { received = msg; return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"session/new","id":2}""");

        Assert.NotNull(received);
        using var doc = JsonDocument.Parse(received);
        var sessionId = doc.RootElement.GetProperty("result").GetProperty("sessionId").GetString();
        Assert.Equal("mock-session-001", sessionId);
    }

    [Fact]
    public async Task SendAsync_SessionPrompt_StreamsChunksThenCompletes()
    {
        await _transport.StartAsync();
        var messages = new List<string>();
        _transport.MessageReceived += msg => { messages.Add(msg); return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"session/prompt","id":3}""");

        // Expect 5 streaming chunks + 1 final response
        Assert.True(messages.Count >= 6, $"Expected at least 6 messages, got {messages.Count}");

        // Last message should be the final response with stopReason
        using var doc = JsonDocument.Parse(messages[^1]);
        var stopReason = doc.RootElement.GetProperty("result").GetProperty("stopReason").GetString();
        Assert.Equal("end_turn", stopReason);
    }

    [Fact]
    public async Task SendAsync_UnknownMethod_ReturnsEmptyResult()
    {
        await _transport.StartAsync();
        string? received = null;
        _transport.MessageReceived += msg => { received = msg; return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"unknown/method","id":99}""");

        Assert.NotNull(received);
        using var doc = JsonDocument.Parse(received);
        Assert.Equal(99, doc.RootElement.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task SendAsync_SessionPrompt_ChunksContainTextContent()
    {
        await _transport.StartAsync();
        var messages = new List<string>();
        _transport.MessageReceived += msg => { messages.Add(msg); return Task.CompletedTask; };

        await _transport.SendAsync("""{"jsonrpc":"2.0","method":"session/prompt","id":4}""");

        // Check that streaming chunks have session/update method with text content
        var chunkMessages = messages.Where(m =>
        {
            using var d = JsonDocument.Parse(m);
            return d.RootElement.TryGetProperty("method", out var method)
                && method.GetString() == "session/update";
        }).ToList();

        Assert.True(chunkMessages.Count >= 5, $"Expected at least 5 chunks, got {chunkMessages.Count}");

        foreach (var chunk in chunkMessages)
        {
            using var d = JsonDocument.Parse(chunk);
            var update = d.RootElement.GetProperty("params").GetProperty("update");
            Assert.Equal("agent_message_chunk", update.GetProperty("sessionUpdate").GetString());
            Assert.Equal("text", update.GetProperty("content").GetProperty("type").GetString());
        }
    }
}
