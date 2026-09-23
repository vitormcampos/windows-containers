using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WindowsContainers.UI.ViewModels;

namespace WindowsContainers.UI;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    private static readonly IReadOnlyDictionary<string, string> ViewMappings =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["WindowsContainers.UI.ViewModels.DashboardViewModel"] = "WindowsContainers.UI.Views.Pages.DashboardPage",
            ["WindowsContainers.UI.ViewModels.SettingsViewModel"] = "WindowsContainers.UI.Views.Pages.SettingsPage",
            ["WindowsContainers.UI.ViewModels.StatusBarViewModel"] = "WindowsContainers.UI.Views.Components.StatusBar",
            ["WindowsContainers.UI.ViewModels.RuntimeSettingsViewModel"] = "WindowsContainers.UI.Views.Components.Settings.RuntimeSettingsSection",
            ["WindowsContainers.UI.ViewModels.SessionSettingsViewModel"] = "WindowsContainers.UI.Views.Components.Settings.SessionSettingsSection",
            ["WindowsContainers.UI.ViewModels.ContainerDefaultsSettingsViewModel"] = "WindowsContainers.UI.Views.Components.Settings.ContainerDefaultsSection",
            ["WindowsContainers.UI.ViewModels.AppearanceSettingsViewModel"] = "WindowsContainers.UI.Views.Components.Settings.AppearanceSettingsSection",
        };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewModelName = param.GetType().FullName!;
        var viewName = ViewMappings.TryGetValue(viewModelName, out var mappedName)
            ? mappedName
            : viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(viewName);

        if (type != null)
            return (Control)Activator.CreateInstance(type)!;

        return new TextBlock { Text = "Not Found: " + viewName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
