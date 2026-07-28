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
        [Route("{zoneIdentifier}")]
        public ActionResult Get(
            string zoneIdentifier)
            => ProcessRequest(
                new GetZoneRequest()
                {
                    Identifier = zoneIdentifier
                },
                () => new GetZoneResponse()
                {
                    Zone = service.GetZone(zoneIdentifier)
                },
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetZonesRequest(),
                () => new GetZonesResponse()
                {
                    Zones = service.GetAllZones()
                },
                authorisation);
    }
}
