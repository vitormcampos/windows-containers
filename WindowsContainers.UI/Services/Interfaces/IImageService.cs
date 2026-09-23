using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IImageService
{
    Task<IReadOnlyList<ImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default);
    Task PullAsync(string image, IProgress<ImagePullProgress>? progress = null, CancellationToken cancellationToken = default);
    Task RemoveImageAsync(string imageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImageInfo>> GetPruneCandidatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImageInfo>> PruneAsync(IReadOnlyCollection<string> imageIds, CancellationToken cancellationToken = default);
}
