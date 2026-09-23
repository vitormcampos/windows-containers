using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IContainerService
{
    Task<IReadOnlyList<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default);
    Task StartAsync(string containerId, CancellationToken cancellationToken = default);
    Task StopAsync(string containerId, CancellationToken cancellationToken = default);
    Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default);
}
