using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetHomesRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string Player { get; set; }

        [HmacOrder(2)]
        public string Name { get; set; }
    }
}