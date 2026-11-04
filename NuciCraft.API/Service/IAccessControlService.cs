using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IAccessControlService
    {
        IpCheckResult CheckIp(CheckIpRequest request);

        UsernameCheckResult CheckUsername(CheckUsernameRequest request);
    }
}