using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int _selectedNavigationIndex;
    private ViewModelBase _currentPage;
    private bool _showStatusBar = true;
    private bool _compactMode;
    private double _sidebarWidth = 240;
    private readonly IThemeService _themeService;
    public IProviderService Provider { get; }

    public MainViewModel(IEnvironmentState environment, SettingsViewModel settings, IThemeService themeService, IProviderService provider)
    {
        Dashboard = new DashboardViewModel(environment);
        Images = new ImagesPageViewModel(provider);
        Settings = settings;
        _themeService = themeService;
        Provider = provider;
        StatusBar = new StatusBarViewModel(environment);
        _currentPage = Dashboard;
        ApplyAppearance(settings.CurrentSettings);
        Settings.SettingsApplied += OnSettingsApplied;
    }

    public DashboardViewModel Dashboard { get; }
    public ImagesPageViewModel Images { get; }
    public SettingsViewModel Settings { get; }
    public StatusBarViewModel StatusBar { get; }

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

    public bool ShowStatusBar
    {
        get => _showStatusBar;
        private set => SetProperty(ref _showStatusBar, value);
    }

    public bool CompactMode
    {
        get => _compactMode;
        private set => SetProperty(ref _compactMode, value);
    }

    public double SidebarWidth
    {
        get => _sidebarWidth;
        private set => SetProperty(ref _sidebarWidth, value);
    }

    private void OnSettingsApplied(object? sender, ApplicationSettings settings) => ApplyAppearance(settings);

    private void ApplyAppearance(ApplicationSettings settings)
    {
        _themeService.Apply(settings.Theme);
        ShowStatusBar = settings.ShowStatusBar;
        CompactMode = settings.CompactMode;
        SidebarWidth = CompactMode ? 200 : 240;
    }
}
