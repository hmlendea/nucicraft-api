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

        [HttpPost]
        public ActionResult Add(
            [FromBody] AddZoneRequest request)
            => ProcessRequest(
                request,
                () => service.Add(request),
                authorisation);

        [HttpGet]
        [Route("{zoneIdentifier}")]
        public ActionResult Get(
            string zoneIdentifier)
            => ProcessRequest(
                new GetZoneRequest()
                {
                    Identifier = zoneIdentifier
                },
                () => new GetResponse(service.GetZone(zoneIdentifier)),
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetZonesRequest(),
                () => new GetResponse(service.GetAllZones()),
                authorisation);

        [HttpPut]
        [Route("{zoneIdentifier}")]
        public ActionResult Update(
            string zoneIdentifier,
            [FromBody] UpdateZoneRequest request)
        {
            request.Identifier = zoneIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
