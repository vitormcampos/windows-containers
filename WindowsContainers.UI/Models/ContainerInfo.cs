namespace WindowsContainers.UI.Models;

public sealed record ContainerInfo(
    string Id,
    string Name,
    string Image,
    ContainerState State,
    ContainerBackend Backend);

public enum ContainerState
{
    Unknown,
    Created,
    Running,
    Exited,
}
