using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class RegisterUserRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Username { get; set; }

        [HmacOrder(2)]
        public string OnlineUUID { get; set; }

        [HmacOrder(3)]
        public string CreatedDT { get; set; }

        [HmacOrder(4)]
        public string Password { get; set; }

        [HmacOrder(5)]
        public string IpAddress { get; set; }

        [HmacOrder(6)]
        public string SkinUrl { get; set; }
    }
}
