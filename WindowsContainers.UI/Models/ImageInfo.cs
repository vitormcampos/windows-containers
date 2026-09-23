using System;

namespace WindowsContainers.UI.Models;

public sealed record ImageInfo(
    string Id,
    string Name,
    string Digest,
    long SizeBytes,
    DateTimeOffset CreatedAt,
    ImageUsageStatus UsageStatus,
    ContainerBackend Backend);

public enum ImageUsageStatus
{
    Unknown,
    Used,
    Unused,
}

public sealed record ImagePullProgress(
    string LayerId,
    string Status,
    long CurrentBytes,
    long TotalBytes);
