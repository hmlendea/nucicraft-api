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
    public sealed class ZoneTypesController(
        IZoneTypeService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add([FromBody] AddZoneTypeRequest request)
            => ProcessRequest(request, () => service.Add(request), authorisation);

        [HttpGet]
        [Route("{zoneTypeIdentifier}")]
        public ActionResult Get(string zoneTypeIdentifier)
            => ProcessRequest(
                new GetZoneTypeRequest { Identifier = zoneTypeIdentifier },
                () => new GetResponse(service.GetZoneType(zoneTypeIdentifier)),
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetZoneTypesRequest(),
                () => new GetResponse(service.GetAllZoneTypes()),
                authorisation);

        [HttpPatch]
        [Route("{zoneTypeIdentifier}")]
        public ActionResult PatchByIdentifier(
            string zoneTypeIdentifier,
            [FromBody] PatchZoneTypeRequest request)
        {
            request.Identifier = zoneTypeIdentifier;

            return ProcessRequest(request, () => service.Update(request), authorisation);
        }
    }
}