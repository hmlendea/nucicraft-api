using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetCountryResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public Country Country { get; set; }
    }
}