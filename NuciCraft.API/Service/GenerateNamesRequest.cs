using System.Text.Json.Serialization;

using NuciAPI.Requests;

namespace NuciCraft.API.Service
{
    public sealed class GenerateNamesRequest : NuciApiRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("schema")]
        public string Schema { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}