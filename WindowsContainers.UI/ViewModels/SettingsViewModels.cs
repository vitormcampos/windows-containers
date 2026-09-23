using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsStore _settingsStore;
    private readonly IPowerShellAliasService _aliasService;
    private readonly IRuntimeInfoService _runtimeInfoService;
    private SettingsSectionViewModel _selectedSection;
    private string _statusMessage = "Settings are stored locally by this application.";

    public event EventHandler<ApplicationSettings>? SettingsApplied;
    public ApplicationSettings CurrentSettings { get; private set; } = new();

    public SettingsViewModel(IAppSettingsStore settingsStore, IPowerShellAliasService aliasService, IRuntimeInfoService runtimeInfoService)
    {
        _settingsStore = settingsStore;
        _aliasService = aliasService;
        _runtimeInfoService = runtimeInfoService;
        Runtime = new RuntimeSettingsViewModel(runtimeInfoService);
        Session = new SessionSettingsViewModel();
        Appearance = new AppearanceSettingsViewModel();
        PowerShell = new PowerShellIntegrationSettingsViewModel(aliasService);
        Sections = new ObservableCollection<SettingsSectionViewModel> { Runtime, Session, Appearance, PowerShell };
        _selectedSection = Runtime;
    }

    public RuntimeSettingsViewModel Runtime { get; }
    public SessionSettingsViewModel Session { get; }
    public AppearanceSettingsViewModel Appearance { get; }
    public PowerShellIntegrationSettingsViewModel PowerShell { get; }
    public ObservableCollection<SettingsSectionViewModel> Sections { get; }

    public SettingsSectionViewModel SelectedSection
    {
        get => _selectedSection;
        set => SetProperty(ref _selectedSection, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            CurrentSettings = await _settingsStore.LoadAsync(cancellationToken);
            PowerShell.Load(CurrentSettings.PowerShellAliases);
            await PowerShell.InspectAsync(CurrentSettings.PowerShellAliases, cancellationToken);
            ApplySettings(CurrentSettings);
            await Runtime.RefreshAsync(cancellationToken);
            StatusMessage = "Application settings loaded.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            StatusMessage = "Could not load application settings. Using defaults.";
        }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        var validationError = ValidateSelectedSection();
        if (validationError is not null)
        {
            StatusMessage = validationError;
            return;
        }

        if (SelectedSection == Runtime)
        {
            StatusMessage = "Runtime information is read-only.";
            return;
        }

        try
        {
            var settings = CurrentSettings;
            var isPowerShellSection = SelectedSection == PowerShell;

            if (SelectedSection == Session)
                settings.SessionName = Session.SessionName;
            else if (SelectedSection == Appearance)
            {
                settings.Theme = Appearance.Theme;
                settings.CompactMode = Appearance.CompactMode;
                settings.ShowStatusBar = Appearance.ShowStatusBar;
            }
            else if (isPowerShellSection)
                settings.PowerShellAliases = PowerShell.ToModels();

            await _settingsStore.SaveAsync(settings);
            CurrentSettings = settings;
            SettingsApplied?.Invoke(this, settings);

            if (!isPowerShellSection)
            {
                StatusMessage = "Section settings saved.";
                return;
            }

            try
            {
                await PowerShell.ApplyAsync();
                StatusMessage = "PowerShell integration settings saved.";
            }
            catch (Exception)
            {
                StatusMessage = "PowerShell settings saved, but aliases could not be applied.";
            }
        }
        catch (Exception)
        {
            StatusMessage = "Could not save section settings.";
        }
    }

    [RelayCommand]
    private void RestoreDefaults()
    {
        foreach (var section in Sections)
            section.RestoreDefaults();

        StatusMessage = "Default values restored. Apply to save these defaults.";
    }

    private string? ValidateSelectedSection()
    {
        if (SelectedSection == Session && Session.SessionName.Contains(' '))
            return "Session name cannot contain spaces.";

        if (SelectedSection == PowerShell && PowerShell.HasValidationError)
            return PowerShell.ValidationError!;

        return null;
    }

    private void ApplySettings(ApplicationSettings settings)
    {
        Session.SessionName = settings.SessionName;
        Appearance.Theme = settings.Theme;
        Appearance.CompactMode = settings.CompactMode;
        Appearance.ShowStatusBar = settings.ShowStatusBar;
    }
}

public abstract class SettingsSectionViewModel : ViewModelBase
{
    protected SettingsSectionViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }
    public string Description { get; }
    public abstract void RestoreDefaults();
}

public sealed partial class RuntimeSettingsViewModel : SettingsSectionViewModel
{
    private readonly IRuntimeInfoService _runtimeInfoService;
    [ObservableProperty] private string status = "Checking…";
    [ObservableProperty] private string runtimeVersion = "Checking…";
    [ObservableProperty] private string session = "Checking…";

    public RuntimeSettingsViewModel(IRuntimeInfoService runtimeInfoService)
        : base("WSL Containers runtime", "Information about the native WSLc runtime used by Windows Containers.")
    {
        _runtimeInfoService = runtimeInfoService;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var info = await _runtimeInfoService.GetInfoAsync(cancellationToken);
        Status = info.Status;
        RuntimeVersion = info.Version;
        Session = info.Session;
    }

    public override void RestoreDefaults() { }
}

public sealed partial class SessionSettingsViewModel : SettingsSectionViewModel
{
    [ObservableProperty] private string sessionName = "";

    public SessionSettingsViewModel() : base("WSLc session", "Select the optional wslc session used for container operations.") { }

    public override void RestoreDefaults() => SessionName = "";
}

public sealed partial class AppearanceSettingsViewModel : SettingsSectionViewModel
{
    public AppearanceSettingsViewModel() : base("Appearance", "Adjust how Windows Containers looks and behaves.") { }
    public IReadOnlyList<string> Themes { get; } = ["System", "Light", "Dark"];
    [ObservableProperty] private string theme = "System";
    [ObservableProperty] private bool compactMode;
    [ObservableProperty] private bool showStatusBar = true;

    public override void RestoreDefaults()
    {
        Theme = "System";
        CompactMode = false;
        ShowStatusBar = true;
    }
}
