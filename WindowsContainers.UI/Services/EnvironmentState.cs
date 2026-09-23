using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

public sealed class EnvironmentState : IEnvironmentState
{
    // Placeholder values until the WSL service supplies real state.
    public string WslStatus { get; } = "Running";
    public int ContainerCount { get; } = 4;
    public int ImageCount { get; } = 6;
}
