using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetRtpLocationResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public RtpLocation RtpLocation { get; set; }
    }
}