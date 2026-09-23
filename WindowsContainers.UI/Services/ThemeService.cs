using Avalonia;
using Avalonia.Styling;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(string theme)
    {
        var application = Application.Current;
        if (application is null)
            return;

        application.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }
}
