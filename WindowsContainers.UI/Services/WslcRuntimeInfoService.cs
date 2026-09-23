using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

public sealed class WslcRuntimeInfoService : IRuntimeInfoService
{
    public async Task<RuntimeInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var version = await TryRunAsync("wslc.exe", ["--version"], cancellationToken);
        var wslStatus = await TryRunAsync("wsl.exe", ["--status"], cancellationToken);

        var status = version.Success && wslStatus.Success ? "Connected" :
            version.Success ? "WSL status unavailable" : "wslc unavailable";
        var versionText = version.Success ? FirstMeaningfulLine(version.Output) : "Not detected";
        var session = version.Success ? "Default wslc session" : "Unavailable";

        return new RuntimeInfo(status, versionText, session);
    }

    private static async Task<CommandResult> TryRunAsync(
        string fileName,
        string[] arguments,
        CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return new CommandResult(process.ExitCode == 0, await outputTask);
        }
        catch (Win32Exception)
        {
            return new CommandResult(false, string.Empty);
        }
    }

    private static string FirstMeaningfulLine(string output)
    {
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }

        return "Detected";
    }

    private sealed record CommandResult(bool Success, string Output);
}
