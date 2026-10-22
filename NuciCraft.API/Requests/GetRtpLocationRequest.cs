using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetRtpLocationRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string Biome { get; set; }

        [HmacOrder(2)]
        public string World { get; set; }
    }
}
