using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IAppSettingsStore
{
    Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default);
}
