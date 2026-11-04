using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

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
        public ActionResult CheckIp(
            [FromQuery] CheckIpRequest request)
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

        [HttpGet]
        [Route("check-username")]
        public ActionResult CheckUsername(
            [FromQuery] CheckUsernameRequest request)
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