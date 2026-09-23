using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

public sealed class PowerShellAliasService : IPowerShellAliasService
{
    private const string WindowsPowerShell = "WindowsPowerShell";
    private const string PowerShell = "PowerShell";
    private static readonly Regex NamePattern = new("^[A-Za-z_][A-Za-z0-9_-]*$", RegexOptions.Compiled);

    public async Task<PowerShellAliasSettings> ApplyAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default)
    {
        Validate(alias);
        await ApplyToProfileAsync(alias, WindowsPowerShell, cancellationToken);
        await ApplyToProfileAsync(alias, PowerShell, cancellationToken);
        return alias;
    }

    public async Task<PowerShellAliasSettings> InspectAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default)
    {
        Validate(alias);
        await InspectProfileAsync(alias, WindowsPowerShell, cancellationToken);
        await InspectProfileAsync(alias, PowerShell, cancellationToken);
        return alias;
    }

    public async Task<PowerShellAliasSettings> RemoveAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default)
    {
        Validate(alias);
        alias.Enabled = false;
        await ApplyToProfileAsync(alias, WindowsPowerShell, cancellationToken);
        await ApplyToProfileAsync(alias, PowerShell, cancellationToken);
        return alias;
    }

    private async Task ApplyToProfileAsync(PowerShellAliasSettings alias, string profile, CancellationToken cancellationToken)
    {
        var path = GetProfilePath(profile);
        var status = GetStatus(alias, profile);
        status.ProfilePath = path;
        status.LastCheckedAt = DateTimeOffset.UtcNow;
        status.LastError = null;

        var content = File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken) : string.Empty;
        var block = GetManagedBlock(alias);
        var withoutBlock = RemoveManagedBlock(content, alias.Name).TrimEnd();

        if (!alias.Enabled)
        {
            if (File.Exists(path))
                await File.WriteAllTextAsync(path, withoutBlock + Environment.NewLine, Encoding.UTF8, cancellationToken);
            status.Status = "Removed";
            return;
        }

        if (HasExternalAlias(withoutBlock, alias.Name))
        {
            status.Status = "Name conflict";
            status.LastError = "An alias with this name already exists outside Windows Containers.";
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, withoutBlock + Environment.NewLine + Environment.NewLine + block + Environment.NewLine, Encoding.UTF8, cancellationToken);
        await InspectProfileAsync(alias, profile, cancellationToken);
    }

    private async Task InspectProfileAsync(PowerShellAliasSettings alias, string profile, CancellationToken cancellationToken)
    {
        var path = GetProfilePath(profile);
        var status = GetStatus(alias, profile);
        status.ProfilePath = path;
        status.LastCheckedAt = DateTimeOffset.UtcNow;
        status.LastError = null;

        if (!File.Exists(path))
        {
            status.Status = "Missing";
            return;
        }

        var content = await File.ReadAllTextAsync(path, cancellationToken);
        if (!content.Contains(GetStartMarker(alias.Name), StringComparison.Ordinal))
        {
            status.Status = HasExternalAlias(content, alias.Name) ? "Name conflict" : "Missing";
            return;
        }

        status.Status = await IsTargetAvailableAsync(alias.Target, profile, cancellationToken)
            ? "Active"
            : "Target unavailable";
    }

    private static async Task<bool> IsTargetAvailableAsync(string target, string profile, CancellationToken cancellationToken)
    {
        var shell = profile == WindowsPowerShell ? "powershell.exe" : "pwsh.exe";
        var escapedTarget = target.Replace("'", "''", StringComparison.Ordinal);
        var startInfo = new ProcessStartInfo(shell)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add($"if (Get-Command -Name '{escapedTarget}' -ErrorAction SilentlyContinue) {{ exit 0 }} else {{ exit 1 }}");

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
                return false;
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested is false)
        {
            return false;
        }
    }

    private static string GetProfilePath(string profile) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        profile,
        "Microsoft.PowerShell_profile.ps1");

    private static string GetManagedBlock(PowerShellAliasSettings alias) =>
        $"{GetStartMarker(alias.Name)}{Environment.NewLine}Set-Alias -Name {alias.Name} -Value {alias.Target}{Environment.NewLine}{GetEndMarker(alias.Name)}";

    private static string GetStartMarker(string name) => $"# BEGIN Windows Containers managed alias: {name}";
    private static string GetEndMarker(string name) => $"# END Windows Containers managed alias: {name}";

    private static string RemoveManagedBlock(string content, string name)
    {
        var start = content.IndexOf(GetStartMarker(name), StringComparison.Ordinal);
        if (start < 0)
            return content;
        var end = content.IndexOf(GetEndMarker(name), start, StringComparison.Ordinal);
        if (end < 0)
            return content;
        end += GetEndMarker(name).Length;
        return content.Remove(start, end - start);
    }

    private static bool HasExternalAlias(string content, string name) =>
        content.Contains($"Set-Alias -Name {name}", StringComparison.OrdinalIgnoreCase) ||
        content.Contains($"New-Alias -Name {name}", StringComparison.OrdinalIgnoreCase);

    private static PowerShellProfileStatus GetStatus(PowerShellAliasSettings alias, string profile) =>
        profile == WindowsPowerShell ? alias.WindowsPowerShell : alias.PowerShell;

    private static void Validate(PowerShellAliasSettings alias)
    {
        if (!NamePattern.IsMatch(alias.Name))
            throw new ArgumentException("The alias name is not valid for PowerShell.", nameof(alias));
        if (string.IsNullOrWhiteSpace(alias.Target) || alias.Target.Any(char.IsWhiteSpace))
            throw new ArgumentException("The alias target must be a command without whitespace.", nameof(alias));
    }
}
