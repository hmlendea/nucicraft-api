using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

namespace NuciCraft.API.Requests
{
    public sealed class CheckIpRequest : NuciApiRequest
    {
        [Required]
        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;
    }
}