using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class ZonesController(
        IZoneService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add(
            [FromBody] AddZoneRequest request)
            => ProcessRequest(
                request,
                () => service.Add(request),
                authorisation);

        [HttpDelete]
        [Route("{zoneIdentifier}")]
        public ActionResult Delete(
            string zoneIdentifier)
            => ProcessRequest(
                new GetZoneRequest()
                {
                    Identifier = zoneIdentifier
                },
                () => service.Delete(zoneIdentifier),
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

        [HttpGet]
        [Route("by-coordinates")]
        public ActionResult GetContainingCoordinates(
            [FromQuery] GetZonesContainingCoordinatesRequest request)
            => ProcessRequest(
                request,
                () => new GetResponse(
                    service.GetZoneIdentifiersContainingCoordinates(new CoordinatesDataObject
                    {
                        World = request.World,
                        X = request.X.Value,
                        Y = request.Y.Value,
                        Z = request.Z.Value
                    })),
                authorisation);

        [HttpPatch]
        [Route("{zoneIdentifier}")]
        public ActionResult PatchByIdentifier(
            string zoneIdentifier,
            [FromBody] PatchZoneRequest request)
        {
            request.Identifier = zoneIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
