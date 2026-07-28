using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public class GetZoneResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public Zone Zone { get; set; }
    }
}
