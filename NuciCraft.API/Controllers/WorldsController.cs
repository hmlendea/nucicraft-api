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
    public sealed class WorldsController(
        IWorldService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add(
            [FromBody] AddWorldRequest request)
            => ProcessRequest(
                request,
                () => service.Add(request),
                authorisation);

        [HttpGet]
        [Route("{worldIdentifier}")]
        public ActionResult Get(
            string worldIdentifier)
            => ProcessRequest(
                new GetWorldRequest()
                {
                    Identifier = worldIdentifier
                },
                () => new GetResponse(service.GetWorld(worldIdentifier)),
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetWorldsRequest(),
                () => new GetResponse(service.GetAllWorlds()),
                authorisation);

        [HttpPatch]
        [Route("{worldIdentifier}")]
        public ActionResult PatchByIdentifier(
            string worldIdentifier,
            [FromBody] PatchWorldRequest request)
        {
            request.Identifier = worldIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
