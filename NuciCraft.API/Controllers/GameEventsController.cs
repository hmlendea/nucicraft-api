using Microsoft.AspNetCore.Mvc;
using NuciAPI.Controllers;
using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GameEventsController(
        IGameEventService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost("player-death")]
        public ActionResult NotifyPlayerDeath(
            [FromBody] NotifyPlayerDeathRequest request)
            => ProcessRequest(
                request,
                () => service.HandlePlayerDeath(request),
                authorisation);
    }
}
