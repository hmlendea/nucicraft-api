using System.Text.Json.Serialization;

using NuciAPI.Responses;

namespace NuciCraft.API.Responses
{
    public sealed class CheckUsernameResponse : NuciApiResponseContent
    {
        [JsonPropertyName("isAllowed")]
        public bool IsAllowed { get; set; }

        [JsonPropertyName("deniedReason")]
        public string? DeniedReason { get; set; }
    }
}