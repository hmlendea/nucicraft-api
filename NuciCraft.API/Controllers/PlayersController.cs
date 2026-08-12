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
    public sealed class PlayersController(
        IPlayerService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Register(
            [FromBody] RegisterPlayerRequest request)
            => ProcessRequest(
                request,
                () => service.Register(request),
                authorisation);

        [HttpGet]
        [Route("{playerIdentifier}")]
        public ActionResult Get(
            string playerIdentifier)
        {
            GetPlayerRequest request = new()
            {
                Identifier = playerIdentifier
            };

            return ProcessRequest(
                request,
                () => new GetPlayerResponse(service.Get(request)),
                authorisation);
        }

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetPlayersRequest(),
                () => new GetResponse(service.GetAll()),
                authorisation);

        [HttpPatch]
        [Route("{playerIdentifier}")]
        public ActionResult PatchByIdentifier(
            string playerIdentifier,
            [FromBody] PatchPlayerRequest request)
        {
            request.PlayerIdentifier = playerIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }

        [HttpPatch]
        [Route("by-username/{username}")]
        public ActionResult PatchByUsername(
            string username,
            [FromBody] PatchPlayerRequest request)
        {
            request.PlayerUsername = username;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }

        [HttpPatch]
        [Route("by-offline-uuid/{offlineUUID}")]
        public ActionResult PatchByOfflineUuid(
            string offlineUUID,
            [FromBody] PatchPlayerRequest request)
        {
            request.PlayerOfflineUUID = offlineUUID;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }

        [HttpPatch]
        [Route("by-online-uuid/{onlineUUID}")]
        public ActionResult PatchByOnlineUuid(
            string onlineUUID,
            [FromBody] PatchPlayerRequest request)
        {
            request.PlayerOnlineUUID = onlineUUID;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
