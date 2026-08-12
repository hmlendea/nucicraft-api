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
        [Route("{identifier}")]
        public ActionResult Get(
            string identifier)
        {
            GetPlayerRequest request = new()
            {
                Identifier = identifier
            };

            return ProcessRequest(
                request,
                () => new GetPlayerResponse(service.Get(request)),
                authorisation);
        }

        [HttpGet]
        [Route("by-username/{username}")]
        public ActionResult GetByUsername(
            string username)
        {
            GetPlayerRequest request = new()
            {
                Username = username
            };

            return ProcessRequest(
                request,
                () => new GetPlayerResponse(service.Get(request)),
                authorisation);
        }

        [HttpGet]
        [Route("by-offline-uuid/{offlineUUID}")]
        public ActionResult GetByOfflineUuid(
            string offlineUUID)
        {
            GetPlayerRequest request = new()
            {
                OfflineUUID = offlineUUID
            };

            return ProcessRequest(
                request,
                () => new GetPlayerResponse(service.Get(request)),
                authorisation);
        }

        [HttpGet]
        [Route("by-online-uuid/{onlineUUID}")]
        public ActionResult GetByOnlineUuid(
            string onlineUUID)
        {
            GetPlayerRequest request = new()
            {
                OnlineUUID = onlineUUID
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
            request.Identifier = playerIdentifier;

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
            request.Username = username;

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
            request.OfflineUUID = offlineUUID;

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
            request.OnlineUUID = onlineUUID;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}
