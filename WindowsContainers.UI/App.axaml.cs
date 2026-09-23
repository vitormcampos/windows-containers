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
        {
            // Composition root: the shared state is created once and injected
            // into every view model that needs it, without passing parameters around.
            IEnvironmentState environment = new EnvironmentState();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(environment),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}