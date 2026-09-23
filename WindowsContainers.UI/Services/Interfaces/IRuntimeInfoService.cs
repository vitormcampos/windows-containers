using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IRuntimeInfoService
{
    Task<RuntimeInfo> GetInfoAsync(CancellationToken cancellationToken = default);
}
