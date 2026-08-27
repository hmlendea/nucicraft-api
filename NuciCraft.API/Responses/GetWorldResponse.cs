using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public class GetWorldResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public World World { get; set; }
    }
}
