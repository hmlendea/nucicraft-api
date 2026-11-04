using System.Text.Json.Serialization;

namespace NuciCraft.API.Service.Models
{
    public sealed class AnonymisationServiceCheckData
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("proxy")]
        public bool IsAnonymisationService { get; set; }
    }
}
