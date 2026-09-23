using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed partial class PowerShellIntegrationSettingsViewModel : SettingsSectionViewModel
{
    private readonly IPowerShellAliasService _aliasService;

    public PowerShellIntegrationSettingsViewModel(IPowerShellAliasService aliasService)
        : base("PowerShell integration", "Manage aliases in Windows PowerShell 5.1 and PowerShell 7 profiles.")
    {
        _aliasService = aliasService;
        Aliases.Add(new PowerShellAliasViewModel());
    }

    public ObservableCollection<PowerShellAliasViewModel> Aliases { get; } = new();

    public void Load(IReadOnlyList<PowerShellAliasSettings> aliases)
    {
        Aliases.Clear();
        foreach (var alias in aliases)
            Aliases.Add(new PowerShellAliasViewModel(alias));

        if (Aliases.Count == 0)
            Aliases.Add(new PowerShellAliasViewModel());
    }

    public async Task InspectAsync(IReadOnlyList<PowerShellAliasSettings> aliases, CancellationToken cancellationToken)
    {
        Load(aliases);
        foreach (var alias in Aliases)
            await _aliasService.InspectAsync(alias.Model, cancellationToken);
        NotifyStatuses();
    }

    public async Task ApplyAsync()
    {
        foreach (var alias in Aliases)
            await _aliasService.ApplyAsync(alias.Model);
        NotifyStatuses();
    }

    public bool HasValidationError => ValidationError is not null;
    public string? ValidationError => ValidateAliases();

    public List<PowerShellAliasSettings> ToModels() =>
        Aliases.Select(alias => alias.Model).ToList();

    private string? ValidateAliases()
    {
        var enabled = Aliases.Where(alias => alias.Enabled).ToList();
        if (enabled.Any(alias => string.IsNullOrWhiteSpace(alias.Name) || string.IsNullOrWhiteSpace(alias.Target)))
            return "Enabled PowerShell aliases require a name and a target command.";

        if (enabled.Select(alias => alias.Name.Trim()).Distinct(System.StringComparer.OrdinalIgnoreCase).Count() != enabled.Count)
            return "PowerShell alias names must be unique.";

        return null;
    }

    [RelayCommand]
    private void AddAlias() => Aliases.Add(new PowerShellAliasViewModel());

    [RelayCommand]
    private void RemoveAlias(PowerShellAliasViewModel alias)
    {
        if (Aliases.Count > 1)
            Aliases.Remove(alias);
        else
            alias.Enabled = false;
    }

    public override void RestoreDefaults()
    {
        Aliases.Clear();
        Aliases.Add(new PowerShellAliasViewModel());
    }

    private void NotifyStatuses()
    {
        foreach (var alias in Aliases)
            alias.NotifyStatusChanged();
    }
}

public sealed class PowerShellAliasViewModel : ViewModelBase
{
    public PowerShellAliasViewModel() : this(new PowerShellAliasSettings()) { }

    public PowerShellAliasViewModel(PowerShellAliasSettings model) => Model = model;

    public PowerShellAliasSettings Model { get; }
    public string Name { get => Model.Name; set { Model.Name = value; OnPropertyChanged(); } }
    public string Target { get => Model.Target; set { Model.Target = value; OnPropertyChanged(); } }
    public bool Enabled { get => Model.Enabled; set { Model.Enabled = value; OnPropertyChanged(); } }
    public string WindowsPowerShellStatus => Model.WindowsPowerShell.Status;
    public string PowerShellStatus => Model.PowerShell.Status;
    public string WindowsPowerShellPath => Model.WindowsPowerShell.ProfilePath;
    public string PowerShellPath => Model.PowerShell.ProfilePath;

    public void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(WindowsPowerShellStatus));
        OnPropertyChanged(nameof(PowerShellStatus));
        OnPropertyChanged(nameof(WindowsPowerShellPath));
        OnPropertyChanged(nameof(PowerShellPath));
    }
}
