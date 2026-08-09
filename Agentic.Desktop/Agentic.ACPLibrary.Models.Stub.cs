namespace Agentic.ACPLibrary.Models;

/// <summary>
/// Stub model representing an available slash command advertised by the agent.
/// This mirrors the minimal shape the UI expects (Name, Description, optional Input metadata).
/// Kept small to avoid coupling with library internals.
/// </summary>
public sealed class AvailableCommand
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public object? Input { get; set; }
}

/// <summary>
/// Notification model for a list of available commands.
/// The real ACPLibrary may provide a richer type; this minimal shape satisfies the UI consumers.
/// </summary>
public sealed class AvailableCommandsUpdate : SessionUpdate
{
    public System.Collections.Generic.IEnumerable<AvailableCommand> AvailableCommands { get; set; } = System.Array.Empty<AvailableCommand>();
}
