using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetRtpLocationResponse(RtpLocation rtpLocation) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Id { get; set; } = rtpLocation.Id;

        [HmacOrder(2)]
        public string Biome { get; set; } = rtpLocation.Biome;

        [HmacOrder(3)]
        public Coordinates Coordinates { get; set; } = rtpLocation.Coordinates;
    }
}