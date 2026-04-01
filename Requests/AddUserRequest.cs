using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class AddUserRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Username { get; set; }

        [HmacOrder(2)]
        public string OnlineUUID { get; set; }

        [HmacOrder(3)]
        public string SkinUrl { get; set; }
    }
}
