using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetRtpLocationRequest : NuciCraftApiRequestBase
    {
        [HmacOrder(1)]
        public string Biome { get; set; }

        [HmacOrder(2)]
        public string World { get; set; }
    }
}
