using Agentic.ACPLibrary.Registry;
using Agentic.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

using Agentic_Desktop;

namespace Agentic.Desktop.ViewModels;

/// <summary>
/// Browses the ACP registry, shows which agents are installed locally (npm / uvx),
/// and launches installed agents through the shared connection flow.
/// </summary>
public partial class RegistryViewModel : ObservableObject
{
    private readonly RegistryService _service;

    /// <summary>Default service wired with the real registry client and locator.</summary>
    private static readonly RegistryService DefaultService = new(new AcpRegistryClient(), new InstalledAgentLocator());

    public RegistryViewModel()
        : this(DefaultService)
    {
    }

    public RegistryViewModel(RegistryService service)
    {
        _service = service;
    }

    public ObservableCollection<RegistryAgentItem> Agents { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLaunch))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(NoSelection))]
    private RegistryAgentItem? _selectedAgent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLaunch))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = string.Empty;

    /// <summary>Launch is allowed only for an installed, selected agent while not busy.</summary>
    public bool CanLaunch => SelectedAgent?.IsInstalled == true && !IsBusy;

    /// <summary>True when a registry agent is selected in the list.</summary>
    public bool HasSelection => SelectedAgent is not null;

    /// <summary>True when no registry agent is selected (empty-state hint).</summary>
    public bool NoSelection => SelectedAgent is null;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusText = LocalizationService.Get("RegistryStatusLoading");

        try
        {
            var agents = await _service.FetchAgentsAsync();
            var installed = await _service.FindInstalledAsync(agents);
            var installedByAgent = installed.ToLookup(i => i.Agent.Id);
            var items = agents.Select(a => new RegistryAgentItem(a)).ToList();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                Agents.Clear();
                foreach (var item in items)
                {
                    item.InstalledInfo = installedByAgent[item.Agent.Id].FirstOrDefault();
                    Agents.Add(item);
                }
                StatusText = agents.Count == 0
                    ? LocalizationService.Get("RegistryStatusEmpty")
                    : LocalizationService.Format("RegistryStatusLoaded", agents.Count);
            });
        }
        catch (Exception ex)
        {
            App.DispatcherQueue.TryEnqueue(() =>
                StatusText = LocalizationService.Format("RegistryStatusError", ex.Message));
        }
        finally
        {
            App.DispatcherQueue.TryEnqueue(() => IsBusy = false);
        }
    }

    [RelayCommand]
    private async Task LaunchAsync()
    {
        var item = SelectedAgent;
        if (item?.InstalledInfo is null || IsBusy) return;

        IsBusy = true;
        StatusText = LocalizationService.Format("RegistryLaunching", item.Name);

        try
        {
            var (command, arguments, env) = RegistryAgentLauncher.BuildLaunchCommand(item.InstalledInfo);
            var settings = SettingsViewModel.Shared;
            settings.AgentPath = command;
            settings.AgentArguments = arguments;
            settings.AgentEnvironment = env;

            await settings.ConnectCommand.ExecuteAsync(null);

            if (settings.IsConnected)
            {
                StatusText = LocalizationService.Format("RegistryLaunchSuccess", item.Name);
                if (App.Window is MainWindow mainWindow)
                {
                    mainWindow.NavigateToChat();
                }
            }
            else
            {
                StatusText = LocalizationService.Format("RegistryLaunchFailed", settings.ConnectionStatus);
            }
        }
        catch (Exception ex)
        {
            StatusText = LocalizationService.Format("RegistryLaunchFailed", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
