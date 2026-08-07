using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetMobNameRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string MobType { get; set; }
    }
}