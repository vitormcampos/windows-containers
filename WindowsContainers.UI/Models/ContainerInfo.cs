using System.Collections.Generic;

namespace WindowsContainers.UI.Models;

public sealed record ContainerInfo(
    string Id,
    string Name,
    string Image,
    ContainerState State,
    IReadOnlyList<PortMapping> Ports,
    IReadOnlyList<VolumeMount> Volumes,
    ContainerBackend Backend);

public sealed record PortMapping(
    string HostAddress,
    string HostPort,
    string ContainerPort,
    string Protocol)
{
    public string Display => $"{HostAddress}:{HostPort}->{ContainerPort}/{Protocol}";
}

public sealed record VolumeMount(
    string Source,
    string Destination,
    string Mode)
{
    public string Display => string.IsNullOrWhiteSpace(Mode)
        ? $"{Source}:{Destination}"
        : $"{Source}:{Destination}:{Mode}";
}

public enum ContainerState
{
    Unknown,
    Created,
    Running,
    Exited,
}
