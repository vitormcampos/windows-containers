using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed partial class ContainersPageViewModel : ViewModelBase
{
    private readonly IContainerService _containerService;
    private string _statusMessage = "Ready to load containers.";
    private string? _errorMessage;
    private string? _activeContainerId;
    private bool _isLoading;
    private bool _isRemoveConfirmationVisible;
    private ContainerItemViewModel? _pendingRemoval;

    public ContainersPageViewModel(IContainerService containerService)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _ = RefreshAsync();
    }

    public ObservableCollection<ContainerItemViewModel> Containers { get; } = [];

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
                OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public bool IsBusy => IsLoading || _activeContainerId is not null;

    public bool IsEmpty => !IsLoading && Containers.Count == 0;

    public bool IsRemoveConfirmationVisible
    {
        get => _isRemoveConfirmationVisible;
        private set => SetProperty(ref _isRemoveConfirmationVisible, value);
    }

    public ContainerItemViewModel? PendingRemoval
    {
        get => _pendingRemoval;
        private set => SetProperty(ref _pendingRemoval, value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Loading containers...";
        try
        {
            var containers = await _containerService.GetContainersAsync();
            Containers.Clear();
            foreach (var container in containers.OrderBy(container => container.Name, StringComparer.OrdinalIgnoreCase))
                Containers.Add(new ContainerItemViewModel(container));

            StatusMessage = $"{Containers.Count} container{(Containers.Count == 1 ? "" : "s")} found.";
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load containers.";
            StatusMessage = "Container loading failed.";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private async Task StartAsync(ContainerItemViewModel? container) => await ExecuteOperationAsync(
        container,
        item => _containerService.StartAsync(item.Id),
        "Starting container...",
        "Container started successfully.",
        "Could not start container.");

    [RelayCommand]
    private async Task StopAsync(ContainerItemViewModel? container) => await ExecuteOperationAsync(
        container,
        item => _containerService.StopAsync(item.Id),
        "Stopping container...",
        "Container stopped successfully.",
        "Could not stop container.");

    [RelayCommand]
    private async Task RestartAsync(ContainerItemViewModel? container) => await ExecuteOperationAsync(
        container,
        item => _containerService.RestartAsync(item.Id),
        "Restarting container...",
        "Container restarted successfully.",
        "Could not restart container.");

    [RelayCommand]
    private void Remove(ContainerItemViewModel? container)
    {
        if (container is null || IsBusy || !container.CanRemove)
            return;

        ErrorMessage = null;
        PendingRemoval = container;
        IsRemoveConfirmationVisible = true;
    }

    [RelayCommand]
    private void CancelRemove()
    {
        IsRemoveConfirmationVisible = false;
        PendingRemoval = null;
    }

    [RelayCommand]
    private async Task ConfirmRemoveAsync()
    {
        var container = PendingRemoval;
        if (container is null || IsBusy)
            return;

        IsRemoveConfirmationVisible = false;
        PendingRemoval = null;
        await ExecuteOperationAsync(
            container,
            item => _containerService.RemoveContainerAsync(item.Id),
            "Removing container...",
            "Container removed successfully.",
            "Could not remove container.");
    }

    private async Task ExecuteOperationAsync(
        ContainerItemViewModel? container,
        Func<ContainerItemViewModel, Task> operation,
        string progressMessage,
        string successMessage,
        string errorMessage)
    {
        if (container is null || IsBusy || !container.CanOperate)
            return;

        _activeContainerId = container.Id;
        ErrorMessage = null;
        StatusMessage = progressMessage;
        OnPropertyChanged(nameof(IsBusy));
        try
        {
            await operation(container);
            StatusMessage = successMessage;
            _activeContainerId = null;
            OnPropertyChanged(nameof(IsBusy));
            await RefreshAsync();
        }
        catch (Exception)
        {
            ErrorMessage = errorMessage;
            StatusMessage = "The container operation failed.";
        }
        finally
        {
            _activeContainerId = null;
            OnPropertyChanged(nameof(IsBusy));
        }
    }
}

public sealed class ContainerItemViewModel : ViewModelBase
{
    private readonly ContainerInfo _container;

    public ContainerItemViewModel(ContainerInfo container) => _container = container;

    public string Id => _container.Id;
    public string Name => string.IsNullOrWhiteSpace(_container.Name) ? _container.Id : _container.Name;
    public string Image => _container.Image;
    public ContainerState State => _container.State;
    public string StateLabel => State.ToString();
    public string Ports => _container.Ports.Count == 0 ? "None" : string.Join(", ", _container.Ports.Select(port => port.Display));
    public string Volumes => _container.Volumes.Count == 0 ? "None" : string.Join(", ", _container.Volumes.Select(volume => volume.Display));
    public bool CanStart => State is ContainerState.Created or ContainerState.Exited;
    public bool CanStop => State == ContainerState.Running;
    public bool CanRestart => State == ContainerState.Running;
    public bool CanRemove => State is ContainerState.Created or ContainerState.Running or ContainerState.Exited;
    public bool CanOperate => State != ContainerState.Unknown;
}
