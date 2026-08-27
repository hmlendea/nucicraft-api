using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetWorldRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }
    }
}
