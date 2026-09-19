using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("Server")]
    [ApiController]
    public sealed class ServersController(
        IServerStatusService serverStatusService,
        ServerSettings serverSettings,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation =
            NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet]
        public ActionResult Get() => ProcessRequest(
            new GetServerRequest(),
            () => new NuciApiContentResponse<GetServerResponse>(new()
            {
                Name = serverSettings.Name,
                Hostname = serverSettings.Hostname,
                JavaEditionPort = serverSettings.JavaEditionPort,
                BedrockEditionPort = serverSettings.BedrockEditionPort,
                OnlinePlayersCount = serverStatusService.GetOnlinePlayersCount()
            }),
            authorisation);
    }
}