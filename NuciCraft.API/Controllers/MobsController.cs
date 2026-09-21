using System.ComponentModel.DataAnnotations;

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
    public sealed class MobsController(
        IMobService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        private readonly NuciApiAuthorisation authorisation =
            NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet("{mobType}/random-name")]
        public ActionResult GetRandomMobName(
            [FromRoute] string mobType,
            [FromQuery, Range(1, 100000)] int? count)
        {
            GetMobNameRequest request = new()
            {
                MobType = mobType
            };

            if (count.HasValue)
            {
                request.Count = count.Value;
            }

            return ProcessRequest(
                request,
                () => new NuciApiContentResponse<GetMobNameResponse>(new()
                {
                    Names = service.GetRandomMobName(request)
                }),
                authorisation);
        }
    }
}