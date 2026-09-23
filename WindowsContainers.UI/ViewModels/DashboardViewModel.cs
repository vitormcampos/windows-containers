using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly IEnvironmentState _environment;

    public DashboardViewModel(IEnvironmentState environment)
    {
        _environment = environment;
    }

    public string Title => "Welcome to Windows Containers";

    public string Description =>
        "Manage Linux containers running on WSL. This dashboard gives you an at-a-glance view of your environment.";

    public string WslStatus => _environment.WslStatus;

    public string ContainersSummary => $"{_environment.ContainerCount} containers";

    public string ImagesSummary => $"{_environment.ImageCount} images";
}
