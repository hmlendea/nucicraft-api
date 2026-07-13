using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;
using NuciCraft.API.Service.Models;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class NotifyPlayerDeathRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Player { get; set; }

        [HmacOrder(2)]
        [Required]
        public Coordinates DeathLocation { get; set; }
    }
}
