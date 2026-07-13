using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Requests
{
    public class UpdatePlayerRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public string Username { get; set; }

        [HmacOrder(3)]
        public string OnlineUUID { get; set; }

        [HmacOrder(4)]
        public string Password { get; set; }

        [HmacOrder(5)]
        public string IpAddress { get; set; }

        [HmacOrder(6)]
        public string DiscordId { get; set; }

        [HmacOrder(7)]
        public string EmailAddress { get; set; }

        [HmacOrder(8)]
        public string LastSleptDT { get; set; }

        [HmacOrder(9)]
        public string LastDeathDT { get; set; }

        [HmacOrder(10)]
        public Coordinates LastDeathLocation { get; set; }

        [HmacOrder(11)]
        public string SkinUrl { get; set; }
    }
}
