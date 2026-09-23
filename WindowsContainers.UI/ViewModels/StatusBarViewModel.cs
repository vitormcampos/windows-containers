using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed class StatusBarViewModel : ViewModelBase
{
    private readonly IEnvironmentState _environment;

    public StatusBarViewModel(IEnvironmentState environment)
    {
        _environment = environment;
    }

    public string WslStatus => _environment.WslStatus;

    public string ContainerSummary => $"Containers: {_environment.ContainerCount}";
}
