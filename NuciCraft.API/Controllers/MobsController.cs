using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class MobsController(
        IMobService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation =
            NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet("random-name")]
        public ActionResult GetRandomMobName([FromQuery] GetMobNameRequest request)
            => ProcessRequest(
                request,
                () => new GetResponse(service.GetRandomMobName(request)),
                authorisation);
    }
}