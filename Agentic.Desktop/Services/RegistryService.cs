using Agentic.ACPLibrary.Registry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agentic.Desktop.Services;

/// <summary>
/// Orchestrates ACP registry index fetching and installed-agent detection.
/// </summary>
public class RegistryService
{
    private readonly IAcpRegistryClient _client;
    private readonly IInstalledAgentLocator _locator;
    private readonly ILogger<RegistryService> _logger;

    public RegistryService(
        IAcpRegistryClient client,
        IInstalledAgentLocator locator,
        ILogger<RegistryService>? logger = null)
    {
        _client = client;
        _locator = locator;
        _logger = logger ?? NullLogger<RegistryService>.Instance;
    }

    /// <summary>Downloads the registry index and returns its agents.</summary>
    public async Task<IReadOnlyList<RegistryAgent>> FetchAgentsAsync(CancellationToken ct = default)
    {
        var index = await _client.FetchIndexAsync(ct);
        _logger.LogInformation("Fetched ACP registry index v{Version} with {Count} agents", index.Version, index.Agents.Count);
        return index.Agents;
    }

    /// <summary>Detects which of the given agents are installed locally (npm global / uv tools).</summary>
    public Task<IReadOnlyList<InstalledAgentInfo>> FindInstalledAsync(
        IReadOnlyList<RegistryAgent> agents, CancellationToken ct = default)
        => _locator.FindInstalledAsync(agents, ct);
}
