using NuciAPI.Responses;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public class GetRtpLocationResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public string Biome { get; set; }

        [HmacOrder(2)]
        public string World { get; set; }

        [HmacOrder(3)]
        public int X { get; set; }

        [HmacOrder(4)]
        public int Y { get; set; }

        [HmacOrder(5)]
        public int Z { get; set; }
    }
}
