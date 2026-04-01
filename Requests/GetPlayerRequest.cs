using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetPlayerRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Username { get; set; }
    }
}
