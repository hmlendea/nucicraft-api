using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetHomeRequest : NuciApiRequest
    {
        [Required]
        [JsonPropertyName("id")]
        [HmacOrder(1)]
        public string Identifier { get; set; }
    }
}