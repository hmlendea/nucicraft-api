using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class AddRtpLocationRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [Required]
        public string Biome { get; set; }

        [HmacOrder(2)]
        [Required]
        public string World { get; set; }

        [HmacOrder(3)]
        [Required]
        public float X { get; set; }

        [HmacOrder(4)]
        [Required]
        public float Y { get; set; }

        [HmacOrder(5)]
        [Required]
        public float Z { get; set; }
    }
}
