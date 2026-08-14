using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class RegisterPlayerRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        public string Username { get; set; }

        [HmacOrder(2)]
        public string DisplayName { get; set; }

        [HmacOrder(3)]
        public string OnlineUUID { get; set; }

        [HmacOrder(4)]
        public string Password { get; set; }

        [HmacOrder(5)]
        public string CreatedDT { get; set; }

        [HmacOrder(6)]
        public string LastIpAddress { get; set; }

        [HmacOrder(7)]
        public string WikiUrl { get; set; }

        [HmacOrder(8)]
        public bool IsBanned { get; set; }

        [HmacOrder(9)]
        public string BannedDT { get; set; }

        [HmacOrder(10)]
        public bool IsMuted { get; set; }

        [HmacOrder(11)]
        public string MutedDT { get; set; }

        [HmacOrder(12)]
        public string LastLoginDT { get; set; }

        [HmacOrder(13)]
        public string LastLogoutDT { get; set; }

        [HmacOrder(14)]
        public CoordinatesDataObject LastLogoutLocation { get; set; }

        [HmacOrder(15)]
        public CoordinatesDataObject BedLocation { get; set; }

        [HmacOrder(16)]
        public string BackDT { get; set; }
    }
}
