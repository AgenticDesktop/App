using System.Text.Json;
using Agentic.ACPLibrary.Transport;

namespace Agentic.Desktop.Mocks;

/// <summary>
/// Mock implementation of <see cref="IAgentTransport"/> providing scripted ACP responses for UI development.
/// </summary>
public sealed class MockAgentTransport : IAgentTransport
{
    private int _requestId;
    private TransportState _state = TransportState.Created;
    private CancellationTokenSource? _promptCts;

    public TransportState State => _state;

    public event Func<string, Task>? MessageReceived;
    public event Func<Exception, Task>? TransportFaulted;
    public event Func<int, Task>? ProcessExited;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _state = TransportState.Running;
        return Task.CompletedTask;
    }

    public async Task SendAsync(string jsonLine, CancellationToken cancellationToken = default)
    {
        if (_state != TransportState.Running)
            return;

        try
        {
            using var doc = JsonDocument.Parse(jsonLine);
            var root = doc.RootElement;
            var method = root.TryGetProperty("method", out var m) ? m.GetString() : null;
            var id = root.TryGetProperty("id", out var i) ? i.GetInt32() : _requestId++;

            switch (method)
            {
                case "initialize":
                    await FireMessageAsync(BuildResponse(id, new
                    {
                        protocolVersion = 1,
                        agentCapabilities = new { },
                        agentInfo = new { name = "mock-agent", title = "Mock Agent", version = "1.0.0" },
                        authMethods = Array.Empty<object>()
                    }));
                    break;

                case "session/new":
                    await FireMessageAsync(BuildResponse(id, new { sessionId = "mock-session-001" }));
                    break;

                case "session/prompt":
                    _promptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    var linkedToken = _promptCts.Token;

                    var chunks = new[]
                    {
                        "Hello! ",
                        "I'm your mock ACP agent. ",
                        "I can help you with coding tasks. ",
                        "This is a streaming response demo. ",
                        "The quick brown fox jumps over the lazy dog. 🚀"
                    };

                    foreach (var chunk in chunks)
                    {
                        linkedToken.ThrowIfCancellationRequested();
                        await Task.Delay(Random.Shared.Next(200, 500), linkedToken);
                        var notification = JsonSerializer.Serialize(new
                        {
                            jsonrpc = "2.0",
                            method = "session/update",
                            @params = new
                            {
                                sessionId = "mock-session-001",
                                update = new
                                {
                                    sessionUpdate = "agent_message_chunk",
                                    messageId = $"msg_{id:D3}",
                                    content = new { type = "text", text = chunk }
                                }
                            }
                        });
                        await FireMessageAsync(notification);
                    }

                    // Send final prompt response
                    await Task.Delay(100, linkedToken);
                    await FireMessageAsync(BuildResponse(id, new { stopReason = "end_turn" }));
                    _promptCts = null;
                    break;

                case "session/cancel":
                    // Cancel any in-progress prompt
                    _promptCts?.Cancel();
                    _promptCts = null;
                    // Notifications don't get a response
                    break;

                default:
                    await FireMessageAsync(BuildResponse(id, new { }));
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Prompt was cancelled, this is expected
        }
        catch (Exception ex)
        {
            if (TransportFaulted is not null)
                await TransportFaulted(ex);
        }
    }

    public Task StopAsync()
    {
        _state = TransportState.Stopped;
        _promptCts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task FireMessageAsync(string json)
    {
        if (MessageReceived is not null)
            await MessageReceived(json);
    }

    private static string BuildResponse(int id, object result)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            result
        });
    }
}
