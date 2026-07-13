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
    public class PlayersController(
        IPlayerService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Register(
            [FromBody] RegisterPlayerRequest request)
            => ProcessRequest(
                request,
                () => service.Register(request),
                authorisation);

        [HttpGet]
        public ActionResult Get(
            [FromQuery] GetPlayerRequest request)
            => ProcessRequest(
                request,
                () => new GetPlayerResponse(service.Get(request)),
                authorisation);

        [HttpPut]
        public ActionResult Update(
            [FromBody] UpdatePlayerRequest request)
            => ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
    }
}
