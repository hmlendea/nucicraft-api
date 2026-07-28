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
    public class ZonesController(
        IZoneService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet]
        public ActionResult Get(
            [FromQuery] GetZoneRequest request)
            => ProcessRequest(
                request,
                () => new GetZoneResponse()
                {
                    Zone = service.GetZone(request.Identifier)
                },
                authorisation);
    }
}
