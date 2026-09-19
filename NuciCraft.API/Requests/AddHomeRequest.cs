using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Requests
{
    public sealed class AddHomeRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        public LocalisedString Name { get; set; }

        [Required]
        [HmacOrder(2)]
        public string Player { get; set; }

        [Required]
        [HmacOrder(3)]
        public Coordinates Location { get; set; }
    }
}