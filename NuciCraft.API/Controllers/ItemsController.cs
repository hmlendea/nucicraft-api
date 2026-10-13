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
    public sealed class ItemsController(
        IItemService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Add(
            [FromBody] AddItemRequest request)
            => ProcessRequest(
                request,
                () => service.Add(request),
                authorisation);

        [HttpGet]
        [Route("{itemIdentifier}")]
        public ActionResult Get(
            string itemIdentifier)
            => ProcessRequest(
                new GetItemRequest()
                {
                    Identifier = itemIdentifier
                },
                () => new NuciApiContentResponse<GetItemResponse>(
                    new GetItemResponse(service.Get(itemIdentifier))),
                authorisation);

        [HttpGet]
        [Route("by-minecraft-id/{minecraftId}")]
        public ActionResult GetByMinecraftId(
            string minecraftId)
            => ProcessRequest(
                new GetItemByMinecraftIdRequest()
                {
                    MinecraftId = minecraftId
                },
                () => new NuciApiContentResponse<GetItemResponse>(
                    new GetItemResponse(service.GetByMinecraftId(minecraftId))),
                authorisation);

        [HttpGet]
        [Route("by-bukkit-id/{bukkitId}")]
        public ActionResult GetByBukkitId(
            string bukkitId)
            => ProcessRequest(
                new GetItemByBukkitIdRequest()
                {
                    BukkitId = bukkitId
                },
                () => new NuciApiContentResponse<GetItemResponse>(
                    new GetItemResponse(service.GetByBukkitId(bukkitId))),
                authorisation);

        [HttpGet]
        [Route("by-nucicraft-id/{nuciCraftId}")]
        public ActionResult GetByNuciCraftId(
            string nuciCraftId)
            => ProcessRequest(
                new GetItemRequest()
                {
                    Identifier = nuciCraftId
                },
                () => new NuciApiContentResponse<GetItemResponse>(
                    new GetItemResponse(service.GetByNuciCraftId(nuciCraftId))),
                authorisation);

        [HttpGet]
        public ActionResult GetAll()
            => ProcessRequest(
                new GetItemsRequest(),
                () => new NuciApiContentResponse<GetItemsResponse>(new()
                {
                    Items = service.GetAll()
                }),
                authorisation);

        [HttpPatch]
        [Route("{itemIdentifier}")]
        public ActionResult PatchByIdentifier(
            string itemIdentifier,
            [FromBody] PatchItemRequest request)
        {
            request.Identifier = itemIdentifier;

            return ProcessRequest(
                request,
                () => service.Update(request),
                authorisation);
        }
    }
}