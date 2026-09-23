using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.ViewModels;

public sealed partial class ImagesPageViewModel : ViewModelBase
{
    private readonly IImageService _imageService;
    private string _imageReference = "";
    private string _statusMessage = "Ready to load images.";
    private string? _errorMessage;
    private bool _isLoading;
    private bool _isPulling;
    private bool _isPrunePreviewVisible;

    public ImagesPageViewModel(IImageService imageService)
    {
        _imageService = imageService;
        _ = RefreshAsync();
    }

    public ObservableCollection<ImageItemViewModel> Images { get; } = new();

    public string ImageReference
    {
        get => _imageReference;
        set => SetProperty(ref _imageReference, value);
    }

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
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsPulling
    {
        get => _isPulling;
        private set => SetProperty(ref _isPulling, value);
    }

    public bool HasImages => Images.Count > 0;
    public ObservableCollection<ImageItemViewModel> PruneCandidates { get; } = new();

    public bool IsPrunePreviewVisible
    {
        get => _isPrunePreviewVisible;
        private set => SetProperty(ref _isPrunePreviewVisible, value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsLoading || IsPulling)
            return;

        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Loading images...";
        try
        {
            var images = await _imageService.GetImagesAsync();
            Images.Clear();
            foreach (var image in images.OrderBy(image => image.Name, StringComparer.OrdinalIgnoreCase))
                Images.Add(new ImageItemViewModel(image));

            OnPropertyChanged(nameof(HasImages));
            StatusMessage = $"{Images.Count} image{(Images.Count == 1 ? "" : "s")} found.";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = "Could not load images.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PullAsync()
    {
        if (string.IsNullOrWhiteSpace(ImageReference) || IsLoading || IsPulling)
            return;

        IsPulling = true;
        ErrorMessage = null;
        StatusMessage = $"Pulling {ImageReference.Trim()}...";
        try
        {
            var reference = ImageReference.Trim();
            await _imageService.PullAsync(reference, new Progress<ImagePullProgress>(progress =>
            {
                StatusMessage = $"{progress.Status}: {progress.LayerId}";
            }));
            ImageReference = "";
            StatusMessage = "Image pulled successfully.";
            IsPulling = false;
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = "Could not pull image.";
        }
        finally
        {
            IsPulling = false;
        }
    }

    [RelayCommand]
    private async Task PreviewPruneAsync()
    {
        if (IsLoading || IsPulling)
            return;

        ErrorMessage = null;
        try
        {
            var candidates = await _imageService.GetPruneCandidatesAsync();
            PruneCandidates.Clear();
            foreach (var image in candidates)
                PruneCandidates.Add(new ImageItemViewModel(image));

            IsPrunePreviewVisible = true;
            StatusMessage = PruneCandidates.Count == 0
                ? "No unused images are available for pruning."
                : $"Review {PruneCandidates.Count} unused image{(PruneCandidates.Count == 1 ? "" : "s")} before removal.";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = "Could not prepare prune preview.";
        }
    }

    [RelayCommand]
    private void CancelPrune()
    {
        IsPrunePreviewVisible = false;
        PruneCandidates.Clear();
        StatusMessage = "Prune cancelled.";
    }

    [RelayCommand]
    private async Task ConfirmPruneAsync()
    {
        if (!IsPrunePreviewVisible || PruneCandidates.Count == 0)
            return;

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var removed = await _imageService.PruneAsync(PruneCandidates.Select(image => image.Id).ToArray());
            IsPrunePreviewVisible = false;
            PruneCandidates.Clear();
            StatusMessage = $"Removed {removed.Count} unused image{(removed.Count == 1 ? "" : "s")}.";
            IsLoading = false;
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = "Could not prune unused images.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveAsync(ImageItemViewModel? image)
    {
        if (image is null || image.UsageStatus == ImageUsageStatus.Used || IsLoading || IsPulling)
            return;

        ErrorMessage = null;
        StatusMessage = $"Removing {image.Name}...";
        try
        {
            await _imageService.RemoveImageAsync(image.Id);
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = $"Could not remove {image.Name}.";
        }
    }
}

public sealed class ImageItemViewModel : ViewModelBase
{
    private readonly ImageInfo _image;

    public ImageItemViewModel(ImageInfo image) => _image = image;

    public string Id => _image.Id;
    public string Name => _image.Name;
    public string Digest => _image.Digest;
    public string Size => FormatSize(_image.SizeBytes);
    public string CreatedAt => _image.CreatedAt == DateTimeOffset.MinValue ? "Unknown" : _image.CreatedAt.ToLocalTime().ToString("g");
    public ImageUsageStatus UsageStatus => _image.UsageStatus;
    public string UsageLabel => UsageStatus switch
    {
        ImageUsageStatus.Used => "Used",
        ImageUsageStatus.Unused => "Unused",
        _ => "Unknown",
    };
    public bool CanRemove => UsageStatus == ImageUsageStatus.Unused;

    private static string FormatSize(long bytes)
    {
        if (bytes <= 0) return "Unknown";
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1) { size /= 1024; unit++; }
        return $"{size:0.#} {units[unit]}";
    }
}
