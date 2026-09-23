using System;

namespace WindowsContainers.UI.Models;

public sealed class PowerShellAliasSettings
{
    public string Name { get; set; } = "container";
    public string Target { get; set; } = "wslc";
    public bool Enabled { get; set; } = true;
    public PowerShellProfileStatus WindowsPowerShell { get; set; } = new();
    public PowerShellProfileStatus PowerShell { get; set; } = new();
}

public sealed class PowerShellProfileStatus
{
    public string Status { get; set; } = "Not checked";
    public string ProfilePath { get; set; } = "";
    public DateTimeOffset? LastCheckedAt { get; set; }
    public string? LastError { get; set; }
}
