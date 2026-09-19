using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class HomesController(
        IHomeService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation =
            NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add([FromBody] AddHomeRequest request) => ProcessRequest(
            request,
            () => new NuciApiContentResponse<GetHomeResponse>(new(service.Add(request))),
            authorisation);

        [HttpGet("{homeIdentifier}")]
        public ActionResult Get(string homeIdentifier) => ProcessRequest(
            new GetHomeRequest { Identifier = homeIdentifier },
            () => new NuciApiContentResponse<GetHomeResponse>(new(service.Get(homeIdentifier))),
            authorisation);

        [HttpGet]
        public ActionResult GetAll([FromQuery] GetHomesRequest request)
        {
            if (request.Name is not null || Request.Query.ContainsKey("name"))
            {
                return ProcessRequest(
                    request,
                    () => new NuciApiContentResponse<GetHomeResponse>(
                        new(service.Get(request.Player, request.Name))),
                    authorisation);
            }

            if (request.Player is not null || Request.Query.ContainsKey("player"))
            {
                return GetAllByPlayer(request.Player);
            }

            return ProcessRequest(
                request,
                () => new NuciApiContentResponse<GetHomesResponse>(new() { Homes = service.GetAll() }),
                authorisation);
        }

        [HttpGet("by-player/{playerIdentifier}")]
        public ActionResult GetAllByPlayer(string playerIdentifier) => ProcessRequest(
            new GetHomesRequest { Player = playerIdentifier },
            () => new NuciApiContentResponse<GetHomesResponse>(new()
            {
                Homes = service.GetAllByPlayer(playerIdentifier)
            }),
            authorisation);

        [HttpPatch("{homeIdentifier}")]
        public ActionResult PatchByIdentifier(
            string homeIdentifier,
            [FromBody] PatchHomeRequest request)
        {
            request.Identifier = homeIdentifier;

            return ProcessRequest(
                request,
                () => new NuciApiContentResponse<GetHomeResponse>(new(service.Update(request))),
                authorisation);
        }
    }
}