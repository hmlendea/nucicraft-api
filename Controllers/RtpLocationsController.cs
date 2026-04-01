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
    public class RtpLocationsController(
        IRtpLocationService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult AddRtpLocation(
            [FromBody] AddRtpLocationRequest request)
            => ProcessRequest(
                request,
                () => service.AddRtpLocation(request),
                authorisation);

        [HttpGet("random")]
        public ActionResult GetRandomRtpLocation(
            [FromQuery] GetRtpLocationRequest request)
            => ProcessRequest(
                request,
                () => new GetResponse(service.GetRtpLocation(request)),
                authorisation);
    }
}
