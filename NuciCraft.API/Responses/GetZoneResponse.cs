using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public class GetZoneRequest : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public Zone Zone { get; set; }
    }
}
