using Agentic.ACPLibrary.Registry;
using Agentic.Desktop.Services;
using Agentic.Desktop.ViewModels;
using Xunit;

namespace Agentic.Desktop.Tests;

public class RegistryServiceTests
{
    private sealed class FakeRegistryClient : IAcpRegistryClient
    {
        public RegistryIndex Index { get; set; } = new();

        public Task<RegistryIndex> FetchIndexAsync(CancellationToken ct = default) => Task.FromResult(Index);
    }

    private sealed class FakeInstalledAgentLocator : IInstalledAgentLocator
    {
        public IReadOnlyList<InstalledAgentInfo> Installed { get; set; } = [];

        public Task<IReadOnlyList<InstalledAgentInfo>> FindInstalledAsync(
            IEnumerable<RegistryAgent> agents, CancellationToken ct = default)
            => Task.FromResult(Installed);
    }

    private static RegistryAgent MakeAgent(string id, string version = "1.0.0") => new()
    {
        Id = id,
        Name = id,
        Version = version,
        Description = $"Description of {id}",
        Repository = $"https://github.com/example/{id}",
        Website = $"https://{id}.example.com",
        Authors = ["Author One", "Author Two"],
        License = "MIT",
        Distribution = new RegistryDistribution
        {
            Npx = new PackageDistributionInfo { Package = $"{id}@{version}" }
        }
    };

    [Fact]
    public async Task FetchAgentsAsync_ReturnsIndexAgents()
    {
        var client = new FakeRegistryClient
        {
            Index = new RegistryIndex
            {
                Version = "1.0.0",
                Agents = [MakeAgent("agent-a"), MakeAgent("agent-b")]
            }
        };
        var service = new RegistryService(client, new FakeInstalledAgentLocator());

        var agents = await service.FetchAgentsAsync();

        Assert.Equal(2, agents.Count);
        Assert.Contains(agents, a => a.Id == "agent-a");
    }

    [Fact]
    public async Task FindInstalledAsync_ReturnsLocatorResults()
    {
        var agent = MakeAgent("agent-a");
        var locator = new FakeInstalledAgentLocator
        {
            Installed = [new InstalledAgentInfo(agent, AgentInstallKind.Npm, "1.0.0", IsUpToDate: true)]
        };
        var service = new RegistryService(new FakeRegistryClient(), locator);

        var installed = await service.FindInstalledAsync([agent]);

        var info = Assert.Single(installed);
        Assert.Equal(AgentInstallKind.Npm, info.Kind);
        Assert.True(info.IsUpToDate);
    }

    [Fact]
    public void RegistryAgentItem_WithoutInstalledInfo_ReportsNotInstalled()
    {
        var item = new RegistryAgentItem(MakeAgent("agent-a"));

        Assert.False(item.IsInstalled);
        Assert.Equal(string.Empty, item.InstalledVersion);
        Assert.False(item.IsUpToDate);
    }

    [Fact]
    public void RegistryAgentItem_WithInstalledInfo_ReportsInstallState()
    {
        var agent = MakeAgent("agent-a", version: "1.0.0");
        var item = new RegistryAgentItem(agent)
        {
            InstalledInfo = new InstalledAgentInfo(agent, AgentInstallKind.Uvx, "0.9.0", IsUpToDate: false)
        };

        Assert.True(item.IsInstalled);
        Assert.Equal("0.9.0", item.InstalledVersion);
        Assert.False(item.IsUpToDate);
    }

    [Fact]
    public void RegistryAgentItem_ExposesMetadataAndUris()
    {
        var item = new RegistryAgentItem(MakeAgent("agent-a"));

        Assert.Equal("agent-a", item.Name);
        Assert.Equal("1.0.0", item.Version);
        Assert.Equal("Author One, Author Two", item.AuthorsText);
        Assert.Equal("MIT", item.License);
        Assert.True(item.HasRepository);
        Assert.True(item.HasWebsite);
        Assert.Equal("https://github.com/example/agent-a", item.RepositoryUri!.ToString());
        Assert.Equal("https://agent-a.example.com/", item.WebsiteUri!.ToString());
    }

    [Fact]
    public void RegistryAgentItem_WithMissingUris_ReportsFalseAndNull()
    {
        var agent = new RegistryAgent
        {
            Id = "no-links",
            Name = "No Links",
            Version = "1.0.0"
        };
        var item = new RegistryAgentItem(agent);

        Assert.False(item.HasRepository);
        Assert.False(item.HasWebsite);
        Assert.Null(item.RepositoryUri);
        Assert.Null(item.WebsiteUri);
    }
}
