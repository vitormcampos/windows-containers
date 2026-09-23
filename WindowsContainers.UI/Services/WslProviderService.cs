using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

/// <summary>
/// WSL Containers provider backed by the native wslc CLI.
/// The provider owns no runtime state; every operation queries or updates wslc directly.
/// </summary>
public sealed class WslProviderService : IProviderService
{
    private readonly IAppSettingsStore _settingsStore;

    public WslProviderService(IAppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public ContainerBackend Backend => ContainerBackend.Wslc;

    public async Task<IReadOnlyList<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
    {
        var output = await ExecuteAsync(["list", "--all", "--format", "json"], cancellationToken);
        return ParseLines(output, ParseContainer).ToList();
    }

    public Task StartAsync(string containerId, CancellationToken cancellationToken = default) =>
        ExecuteCommandAsync(["start", containerId], cancellationToken);

    public Task StopAsync(string containerId, CancellationToken cancellationToken = default) =>
        ExecuteCommandAsync(["stop", containerId], cancellationToken);

    public Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default) =>
        ExecuteCommandAsync(["remove", containerId], cancellationToken);

    public async Task<IReadOnlyList<ImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        var containers = await GetContainersAsync(cancellationToken);
        var usedImages = containers
            .Where(container => container.State is ContainerState.Created or ContainerState.Running)
            .Select(container => container.Image)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var output = await ExecuteAsync(["images", "--all", "--format", "json"], cancellationToken);
        return ParseLines(output, element => ParseImage(element, usedImages)).ToList();
    }

    public async Task PullAsync(
        string image,
        IProgress<ImagePullProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(image))
            throw new ArgumentException("An image reference is required.", nameof(image));

        progress?.Report(new ImagePullProgress(image, "Pulling", 0, 0));
        await ExecuteCommandAsync(["pull", image], cancellationToken);
        progress?.Report(new ImagePullProgress(image, "Complete", 0, 0));
    }

    public Task RemoveImageAsync(string imageId, CancellationToken cancellationToken = default) =>
        ExecuteCommandAsync(["rmi", imageId], cancellationToken);

    public async Task<IReadOnlyList<ImageInfo>> GetPruneCandidatesAsync(CancellationToken cancellationToken = default)
    {
        var images = await GetImagesAsync(cancellationToken);
        return images.Where(image => image.UsageStatus == ImageUsageStatus.Unused).ToList();
    }

    public async Task<IReadOnlyList<ImageInfo>> PruneAsync(IReadOnlyCollection<string> imageIds, CancellationToken cancellationToken = default)
    {
        // Re-query immediately before deleting. Unknown or newly used images are never removed.
        var candidates = await GetPruneCandidatesAsync(cancellationToken);
        var allowed = candidates.Where(image => imageIds.Contains(image.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        var removed = new List<ImageInfo>();
        foreach (var image in allowed)
        {
            await RemoveImageAsync(image.Id, cancellationToken);
            removed.Add(image);
        }
        return removed;
    }

    private async Task ExecuteCommandAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        _ = await ExecuteAsync(arguments, cancellationToken);
    }

    private async Task<string> ExecuteAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var settings = await _settingsStore.LoadAsync(cancellationToken);
        try
        {
            return await RunCommandAsync(arguments, settings.SessionName, cancellationToken);
        }
        catch (InvalidOperationException exception) when (
            !string.IsNullOrWhiteSpace(settings.SessionName) &&
            exception.Message.Contains("WSLC_E_SESSION_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
        {
            // A missing optional session falls back to wslc's default session.
            // This keeps older application defaults from hiding the current runtime.
            return await RunCommandAsync(arguments, null, cancellationToken);
        }
    }

    private static async Task<string> RunCommandAsync(IReadOnlyList<string> arguments, string? sessionName, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wslc.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        // wslc global options must precede the subcommand.
        if (!string.IsNullOrWhiteSpace(sessionName))
        {
            process.StartInfo.ArgumentList.Add("--session");
            process.StartInfo.ArgumentList.Add(sessionName);
        }

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "The wslc command failed." : error.Trim());

        return output;
    }

    private static IEnumerable<T> ParseLines<T>(string output, Func<JsonElement, T> parser)
    {
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            using var document = JsonDocument.Parse(line);
            yield return parser(document.RootElement);
        }
    }

    private static ContainerInfo ParseContainer(JsonElement element)
    {
        var state = element.TryGetProperty("State", out var stateElement)
            ? stateElement.GetString()?.ToLowerInvariant() switch
            {
                "running" => ContainerState.Running,
                "created" => ContainerState.Created,
                "exited" or "dead" => ContainerState.Exited,
                _ => ContainerState.Unknown,
            }
            : ContainerState.Unknown;

        return new ContainerInfo(
            GetString(element, "ID"),
            GetString(element, "Names"),
            GetString(element, "Image"),
            state,
            ContainerBackend.Wslc);
    }

    private static ImageInfo ParseImage(JsonElement element, IReadOnlySet<string> usedImages)
    {
        var repository = GetString(element, "Repository");
        var tag = GetString(element, "Tag");
        var name = string.IsNullOrWhiteSpace(tag) || tag == "<none>" ? repository : $"{repository}:{tag}";
        var created = DateTimeOffset.TryParse(GetString(element, "CreatedAt"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed
            : DateTimeOffset.MinValue;

        var usageStatus = GetUsageStatus(element, name, usedImages);
        return new ImageInfo(
            GetString(element, "ID"),
            name,
            GetString(element, "Digest"),
            ParseSize(GetString(element, "Size")),
            created,
            usageStatus,
            ContainerBackend.Wslc);
    }

    private static ImageUsageStatus GetUsageStatus(JsonElement element, string imageName, IReadOnlySet<string> usedImages)
    {
        if (usedImages.Contains(imageName))
            return ImageUsageStatus.Used;

        // wslc reports the number of containers referencing each image.
        // This is more reliable than comparing image strings alone.
        if (element.TryGetProperty("Containers", out var containersElement) &&
            int.TryParse(containersElement.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var containerCount))
        {
            return containerCount > 0 ? ImageUsageStatus.Used : ImageUsageStatus.Unused;
        }

        // Never classify an image as safe to prune when usage could not be verified.
        return ImageUsageStatus.Unknown;
    }

    private static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) ? value.ToString() : string.Empty;

    private static long ParseSize(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "N/A")
            return 0;

        var units = new[] { ("GB", 1024d * 1024 * 1024), ("MB", 1024d * 1024), ("KB", 1024d), ("B", 1d) };
        foreach (var (unit, multiplier) in units)
        {
            if (!value.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
                continue;
            var number = value[..^unit.Length].Trim();
            return double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var size)
                ? (long)(size * multiplier)
                : 0;
        }

        return 0;
    }
}
