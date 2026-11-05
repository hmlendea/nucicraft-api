using System.Threading;
using System.Threading.Tasks;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IAccessControlService
    {
        Task<IpCheckResult> CheckIpAsync(CheckIpRequest request);

        Task<IpCheckResult> CheckIpAsync(CheckIpRequest request, CancellationToken cancellationToken);

        Task<UsernameCheckResult> CheckUsernameAsync(CheckUsernameRequest request);

        Task<UsernameCheckResult> CheckUsernameAsync(CheckUsernameRequest request, CancellationToken cancellationToken);
    }
}