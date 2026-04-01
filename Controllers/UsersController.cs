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
    public class UsersController(
        IUserService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpPost]
        public ActionResult Register(
            [FromBody] RegisterUserRequest request)
            => ProcessRequest(
                request,
                () => service.Register(request),
                authorisation);

        [HttpGet("{username}")]
        public ActionResult GetUser(
            [FromRoute] string username)
        {
            GetUserRequest request = new()
            {
                Username = username
            };

            return ProcessRequest(
                request,
                () => new GetResponse(service.Get(request.Username)),
                authorisation);
        }
    }
}
