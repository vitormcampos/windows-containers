namespace WindowsContainers.UI.Services.Interfaces;

/// <summary>
/// Shared, read-only snapshot of the WSL/container environment.
/// Populated by the WSL service in future iterations.
/// </summary>
public interface IEnvironmentState
{
    string WslStatus { get; }
    int ContainerCount { get; }
    int ImageCount { get; }
}
