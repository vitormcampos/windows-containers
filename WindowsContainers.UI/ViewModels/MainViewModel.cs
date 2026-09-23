using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int _selectedNavigationIndex;
    private ViewModelBase _currentPage;

    public MainViewModel(IEnvironmentState environment, SettingsViewModel settings, IProviderService provider)
    {
        Dashboard = new DashboardViewModel(environment);
        Images = new ImagesPageViewModel(provider);
        Settings = settings;
        Provider = provider;
        StatusBar = new StatusBarViewModel(environment);
        _currentPage = Dashboard;
    }

    public DashboardViewModel Dashboard { get; }
    public ImagesPageViewModel Images { get; }
    public SettingsViewModel Settings { get; }
    public StatusBarViewModel StatusBar { get; }
    public IProviderService Provider { get; }

    public int SelectedNavigationIndex
    {
        get => _selectedNavigationIndex;
        set
        {
            if (!SetProperty(ref _selectedNavigationIndex, value))
                return;

            CurrentPage = value switch
            {
                2 => Images,
                3 => Settings,
                _ => Dashboard,
            };
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }
}
