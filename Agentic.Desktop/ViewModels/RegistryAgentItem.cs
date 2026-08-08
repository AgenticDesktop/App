using Agentic.ACPLibrary.Registry;
using Agentic.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Agentic.Desktop.ViewModels;

/// <summary>Display model for a single ACP registry agent, enriched with local installation state.</summary>
public partial class RegistryAgentItem : ObservableObject
{
    public RegistryAgent Agent { get; }

    public RegistryAgentItem(RegistryAgent agent)
    {
        Agent = agent;
    }

    /// <summary>Local installation info; null when the agent is not installed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInstalled))]
    [NotifyPropertyChangedFor(nameof(InstalledVersion))]
    [NotifyPropertyChangedFor(nameof(IsUpToDate))]
    [NotifyPropertyChangedFor(nameof(InstallKindText))]
    private InstalledAgentInfo? _installedInfo;

    public string Name => Agent.Name;

    public string Version => Agent.Version;

    public string Description => Agent.Description;

    public string? License => Agent.License;

    public string? Repository => Agent.Repository;

    public string? Website => Agent.Website;

    public string? IconUrl => Agent.Icon;

    /// <summary>True when the agent has a non-empty icon URL.</summary>
    public bool HasIcon => !string.IsNullOrWhiteSpace(Agent.Icon);

    /// <summary>First character of the agent name, used as a fallback when no icon is available.</summary>
    public string Initials => string.IsNullOrEmpty(Name) ? "?" : Name[..1].ToUpperInvariant();

    public string AuthorsText => string.Join(", ", Agent.Authors);

    public bool HasRepository => !string.IsNullOrWhiteSpace(Agent.Repository);

    public bool HasWebsite => !string.IsNullOrWhiteSpace(Agent.Website);

    public Uri? RepositoryUri => TryCreateUri(Agent.Repository);

    public Uri? WebsiteUri => TryCreateUri(Agent.Website);

    private static Uri? TryCreateUri(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;

    public bool IsInstalled => InstalledInfo is not null;

    public string InstalledVersion => InstalledInfo?.InstalledVersion ?? string.Empty;

    public bool IsUpToDate => InstalledInfo?.IsUpToDate ?? false;

    /// <summary>Localized badge text for the installation channel (npm / uvx); null when not installed.</summary>
    public string? InstallKindText => InstalledInfo?.Kind switch
    {
        AgentInstallKind.Npm => LocalizationService.Get("RegistryNpmBadge"),
        AgentInstallKind.Uvx => LocalizationService.Get("RegistryUvxBadge"),
        _ => null
    };
}
