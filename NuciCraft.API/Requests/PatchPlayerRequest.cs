using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class PatchPlayerRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string PlayerIdentifier { get; set; }

        [HmacOrder(2)]
        public string PlayerUsername { get; set; }

        [HmacOrder(3)]
        public string PlayerOfflineUUID { get; set; }

        [HmacOrder(4)]
        public string PlayerOnlineUUID { get; set; }

        [HmacOrder(5)]
        public string Password { get; set; }

        [HmacOrder(6)]
        public string IpAddress { get; set; }

        [HmacOrder(7)]
        public string DiscordId { get; set; }

        [HmacOrder(8)]
        public string EmailAddress { get; set; }

        [HmacOrder(9)]
        public string LastSleptDT { get; set; }

        [HmacOrder(10)]
        public string LastDeathDT { get; set; }

        [HmacOrder(11)]
        public CoordinatesDataObject LastDeathLocation { get; set; }

        [HmacOrder(12)]
        public CoordinatesDataObject BackLocation { get; set; }

        [HmacOrder(13)]
        public CoordinatesDataObject LogoutLocation { get; set; }

        [HmacOrder(14)]
        public PlayerSettingsDataObject Settings { get; set; }
    }
}
