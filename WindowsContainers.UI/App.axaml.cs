using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WindowsContainers.UI.Services;
using WindowsContainers.UI.Services.Interfaces;
using WindowsContainers.UI.ViewModels;
using WindowsContainers.UI.Views;

namespace WindowsContainers.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _ = InitializeDesktopAsync(desktop);

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeDesktopAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Composition root: runtime services and application-owned storage remain separate.
        IEnvironmentState environment = new EnvironmentState();
        IAppSettingsStore settingsStore = new SqliteAppSettingsStore();
        IThemeService themeService = new ThemeService();
        IPowerShellAliasService aliasService = new PowerShellAliasService();
        IRuntimeInfoService runtimeInfoService = new WslcRuntimeInfoService();
        IProviderService providerService = new WslProviderService(settingsStore);
        var settings = new SettingsViewModel(settingsStore, aliasService, runtimeInfoService);
        await settings.LoadAsync();

        desktop.MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(environment, settings, themeService, providerService),
        };
        desktop.MainWindow.Show();
    }
}