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

        [HttpPost]
        [Route("check-ip")]
        public ActionResult CheckIp(
            [FromBody] CheckIpRequest request)
            => ProcessRequest(
                request,
                () => new NuciApiContentResponse<CheckIpResponse>(
                    new CheckIpResponse
                    {
                        IsAllowed = service.CheckIp(request).IsAllowed,
                        DeniedReason = service.CheckIp(request).DeniedReason,
                        CountryCode = service.CheckIp(request).CountryCode
                    }),
                authorisation);

        [HttpPost]
        [Route("check-username")]
        public ActionResult CheckUsername(
            [FromBody] CheckUsernameRequest request)
            => ProcessRequest(
                request,
                () => new NuciApiContentResponse<CheckUsernameResponse>(
                    new CheckUsernameResponse
                    {
                        IsAllowed = service.CheckUsername(request).IsAllowed,
                        DeniedReason = service.CheckUsername(request).DeniedReason
                    }),
                authorisation);
    }
}