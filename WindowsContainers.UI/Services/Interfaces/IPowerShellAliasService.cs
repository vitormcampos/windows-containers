using System.Threading;
using System.Threading.Tasks;
using WindowsContainers.UI.Models;

namespace WindowsContainers.UI.Services.Interfaces;

public interface IPowerShellAliasService
{
    Task<PowerShellAliasSettings> ApplyAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default);
    Task<PowerShellAliasSettings> InspectAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default);
    Task<PowerShellAliasSettings> RemoveAsync(PowerShellAliasSettings alias, CancellationToken cancellationToken = default);
}
