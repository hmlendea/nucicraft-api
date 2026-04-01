using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public abstract class NuciCraftApiRequestBase : NuciApiRequest
    {
        [HmacOrder(0)]
        [Required]
        public string Username { get; set; }
    }
}
