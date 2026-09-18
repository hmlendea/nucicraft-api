using NuciAPI.Responses;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public sealed class GetMobNameResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Name { get; set; }
    }
}