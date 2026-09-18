using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetZoneTypeResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public ZoneType ZoneType { get; set; }
    }
}