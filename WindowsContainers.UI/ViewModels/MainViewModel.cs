using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int _selectedNavigationIndex;
    private ViewModelBase _currentPage;

    public MainViewModel(IEnvironmentState environment)
    {
        Dashboard = new DashboardViewModel(environment);
        Settings = new SettingsViewModel();
        StatusBar = new StatusBarViewModel(environment);
        _currentPage = Dashboard;
    }

    public DashboardViewModel Dashboard { get; }
    public SettingsViewModel Settings { get; }
    public StatusBarViewModel StatusBar { get; }

    public int SelectedNavigationIndex
    {
        get => _selectedNavigationIndex;
        set
        {
            if (!SetProperty(ref _selectedNavigationIndex, value))
                return;

            CurrentPage = value == 3 ? Settings : Dashboard;
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }
}
