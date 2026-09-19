using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetZonesByCategoryRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        public string Category { get; set; }
    }
}
