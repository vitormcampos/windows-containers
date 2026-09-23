using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel(IEnvironmentState environment)
    {
        Dashboard = new DashboardViewModel(environment);
        StatusBar = new StatusBarViewModel(environment);
    }

    public DashboardViewModel Dashboard { get; }

    public StatusBarViewModel StatusBar { get; }
}
