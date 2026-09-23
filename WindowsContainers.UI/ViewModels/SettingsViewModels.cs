using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace WindowsContainers.UI.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private SettingsSectionViewModel _selectedSection;
    private string _statusMessage = "Changes are local to this session.";

    public SettingsViewModel()
    {
        Sections = new ObservableCollection<SettingsSectionViewModel>
        {
            new RuntimeSettingsViewModel(),
            new SessionSettingsViewModel(),
            new ContainerDefaultsSettingsViewModel(),
            new AppearanceSettingsViewModel(),
        };
        _selectedSection = Sections[0];
    }

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

    [RelayCommand]
    private void Apply()
    {
        StatusMessage = "Changes applied for this session.";
    }

    [RelayCommand]
    private void RestoreDefaults()
    {
        foreach (var section in Sections)
            section.RestoreDefaults();

        StatusMessage = "Default values restored.";
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

public sealed class RuntimeSettingsViewModel : SettingsSectionViewModel
{
    public RuntimeSettingsViewModel() : base("wslc runtime", "Configure the native WSL-powered container runtime.") { }
    public string Status => "Not connected";
    public string RuntimeVersion => "Not detected";
    public bool GpuEnabled { get; set; }
    public bool AutoStartSession { get; set; }
    public override void RestoreDefaults()
    {
        GpuEnabled = false; AutoStartSession = false;
        OnPropertyChanged(nameof(GpuEnabled)); OnPropertyChanged(nameof(AutoStartSession));
    }
}

public sealed class SessionSettingsViewModel : SettingsSectionViewModel
{
    public SessionSettingsViewModel() : base("Container session", "Define the resources and storage used by a wslc container session.") { }
    public string SessionName { get; set; } = "windows-containers";
    public string StoragePath { get; set; } = "%LOCALAPPDATA%\\WindowsContainers\\sessions";
    public int CpuCount { get; set; } = 4;
    public int MemoryMb { get; set; } = 4096;
    public int TimeoutSeconds { get; set; } = 30;
    public bool GpuEnabled { get; set; }
    public string VhdType { get; set; } = "Dynamic";
    public int VhdSizeGb { get; set; } = 64;
    public override void RestoreDefaults()
    {
        SessionName = "windows-containers"; StoragePath = "%LOCALAPPDATA%\\WindowsContainers\\sessions";
        CpuCount = 4; MemoryMb = 4096; TimeoutSeconds = 30; GpuEnabled = false; VhdType = "Dynamic"; VhdSizeGb = 64;
        OnPropertyChanged(nameof(SessionName)); OnPropertyChanged(nameof(StoragePath)); OnPropertyChanged(nameof(CpuCount)); OnPropertyChanged(nameof(MemoryMb));
        OnPropertyChanged(nameof(TimeoutSeconds)); OnPropertyChanged(nameof(GpuEnabled)); OnPropertyChanged(nameof(VhdType)); OnPropertyChanged(nameof(VhdSizeGb));
    }
}

public sealed class ContainerDefaultsSettingsViewModel : SettingsSectionViewModel
{
    public ContainerDefaultsSettingsViewModel() : base("Container defaults", "Defaults used when creating new containers.") { }
    public string Image { get; set; } = "ubuntu:24.04";
    public string NetworkMode { get; set; } = "Bridged";
    public string HostName { get; set; } = "";
    public string DomainName { get; set; } = "";
    public bool AutoRemove { get; set; }
    public bool Privileged { get; set; }
    public bool GpuEnabled { get; set; }
    public string WorkingDirectory { get; set; } = "/workspace";
    public override void RestoreDefaults()
    {
        Image = "ubuntu:24.04"; NetworkMode = "Bridged"; HostName = ""; DomainName = "";
        AutoRemove = false; Privileged = false; GpuEnabled = false; WorkingDirectory = "/workspace";
        OnPropertyChanged(nameof(Image)); OnPropertyChanged(nameof(NetworkMode)); OnPropertyChanged(nameof(HostName)); OnPropertyChanged(nameof(DomainName));
        OnPropertyChanged(nameof(AutoRemove)); OnPropertyChanged(nameof(Privileged)); OnPropertyChanged(nameof(GpuEnabled)); OnPropertyChanged(nameof(WorkingDirectory));
    }
}

public sealed class AppearanceSettingsViewModel : SettingsSectionViewModel
{
    public AppearanceSettingsViewModel() : base("Appearance", "Adjust how Windows Containers looks and behaves.") { }
    public string Theme { get; set; } = "System";
    public bool CompactMode { get; set; }
    public bool ShowStatusBar { get; set; } = true;
    public override void RestoreDefaults()
    {
        Theme = "System"; CompactMode = false; ShowStatusBar = true;
        OnPropertyChanged(nameof(Theme)); OnPropertyChanged(nameof(CompactMode)); OnPropertyChanged(nameof(ShowStatusBar));
    }
}
