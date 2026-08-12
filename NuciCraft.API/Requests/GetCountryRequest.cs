using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetCountryRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string Identifier { get; set; }
    }
}
