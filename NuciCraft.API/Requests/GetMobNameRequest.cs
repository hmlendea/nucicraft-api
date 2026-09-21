using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class GetMobNameRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("type")]
        public string MobType { get; set; }

        [HmacOrder(2)]
        [Range(1, 100000)]
        [JsonPropertyName("count")]
        public int Count { get; set; } = DefaultCount;

        private static int DefaultCount => 1;
    }
}