using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace NuciCraft.API.Service
{
    public sealed class GenerateNamesRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("schema")]
        public string Schema { get; set; }

        [HmacOrder(2)]
        [Range(1, 100000)]
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}