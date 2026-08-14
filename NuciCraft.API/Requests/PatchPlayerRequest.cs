using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class PatchPlayerRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public string Username { get; set; }

        [HmacOrder(3)]
        public string OfflineUUID { get; set; }

        [HmacOrder(4)]
        public string OnlineUUID { get; set; }

        [HmacOrder(5)]
        public string Password { get; set; }

        [HmacOrder(6)]
        public string LastIpAddress { get; set; }

        [HmacOrder(7)]
        public string DiscordId { get; set; }

        [HmacOrder(8)]
        public string EmailAddress { get; set; }

        [HmacOrder(9)]
        public string WikiUrl { get; set; }

        [HmacOrder(10)]
        public bool IsBanned { get; set; }

        [HmacOrder(11)]
        public string BannedDT { get; set; }

        [HmacOrder(12)]
        public bool IsMuted { get; set; }

        [HmacOrder(13)]
        public string MutedDT { get; set; }

        [HmacOrder(14)]
        public string LastLoginDT { get; set; }

        [HmacOrder(15)]
        public string LastLogoutDT { get; set; }

        [HmacOrder(16)]
        public CoordinatesDataObject LastLogoutLocation { get; set; }

        [HmacOrder(17)]
        public string LastSleptDT { get; set; }

        [HmacOrder(18)]
        public CoordinatesDataObject BedLocation { get; set; }

        [HmacOrder(19)]
        public string LastDeathDT { get; set; }

        [HmacOrder(20)]
        public CoordinatesDataObject LastDeathLocation { get; set; }

        [HmacOrder(21)]
        public string BackDT { get; set; }

        [HmacOrder(22)]
        public CoordinatesDataObject BackLocation { get; set; }

        [HmacOrder(23)]
        public PlayerSettingsDataObject Settings { get; set; }
    }
}
