using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetPlayerRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public string Username { get; set; }

        [HmacOrder(3)]
        public string OfflineUUID { get; set; }

        [HmacOrder(4)]
        public string OnlineUUID { get; set; }
    }
}
