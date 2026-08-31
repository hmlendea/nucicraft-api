using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetZonesContainingCoordinatesRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        public string World { get; set; }

        [Required]
        [HmacOrder(2)]
        public float? X { get; set; }

        [Required]
        [HmacOrder(3)]
        public float? Y { get; set; }

        [Required]
        [HmacOrder(4)]
        public float? Z { get; set; }
    }
}