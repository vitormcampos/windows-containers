using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IProviderService : IContainerService, IImageService
{
    ContainerBackend Backend { get; }
}
