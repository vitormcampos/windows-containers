using System.Collections.Generic;

namespace WindowsContainers.UI.Models;

/// <summary>
/// Application-owned preferences and defaults. Runtime state is never persisted here.
/// </summary>
public sealed class ApplicationSettings
{
    public string Theme { get; set; } = "System";
    public bool CompactMode { get; set; }
    public bool ShowStatusBar { get; set; } = true;

    public bool RuntimeGpuEnabled { get; set; }
    public bool AutoStartSession { get; set; }

    public string SessionName { get; set; } = "";
    public string StoragePath { get; set; } = "%LOCALAPPDATA%\\WindowsContainers\\sessions";
    public int CpuCount { get; set; } = 4;
    public int MemoryMb { get; set; } = 4096;
    public int TimeoutSeconds { get; set; } = 30;
    public bool SessionGpuEnabled { get; set; }
    public string VhdType { get; set; } = "Dynamic";
    public int VhdSizeGb { get; set; } = 64;

    public string DefaultImage { get; set; } = "ubuntu:24.04";
    public string NetworkMode { get; set; } = "Bridged";
    public string HostName { get; set; } = "";
    public string DomainName { get; set; } = "";
    public string WorkingDirectory { get; set; } = "/workspace";
    public bool AutoRemove { get; set; }
    public bool Privileged { get; set; }
    public bool ContainerGpuEnabled { get; set; }
    public List<PowerShellAliasSettings> PowerShellAliases { get; set; } = new();
}
