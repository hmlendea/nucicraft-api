using Microsoft.AspNetCore.Mvc;
using NuciAPI.Controllers;
using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
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
        public ActionResult AddUser(
            [FromBody] AddUserRequest request)
            => ProcessRequest(
                request,
                () => service.AddUser(request),
                authorisation);
    }
}
