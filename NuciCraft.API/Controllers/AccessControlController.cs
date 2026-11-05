using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class AccessControlController(
        IAccessControlService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet]
        [Route("check-ip")]
        public async Task<ActionResult> CheckIp(
            [FromQuery] CheckIpRequest request,
            CancellationToken cancellationToken)
        {
            IpCheckResult result = await service.CheckIpAsync(request, cancellationToken);

            return ProcessRequest(
                request,
                () => new NuciApiContentResponse<CheckIpResponse>(
                    new CheckIpResponse
                    {
                        IsAllowed = result.IsAllowed,
                        DeniedReason = result.DeniedReason,
                        CountryCode = result.CountryCode
                    }),
                authorisation);
        }

        [HttpGet]
        [Route("check-username")]
        public async Task<ActionResult> CheckUsername(
            [FromQuery] CheckUsernameRequest request,
            CancellationToken cancellationToken)
        {
            UsernameCheckResult result = await service.CheckUsernameAsync(request, cancellationToken);

            return ProcessRequest(
                request,
                () => new NuciApiContentResponse<CheckUsernameResponse>(
                    new CheckUsernameResponse
                    {
                        IsAllowed = result.IsAllowed,
                        DeniedReason = result.DeniedReason
                    }),
                authorisation);
        }
    }
}