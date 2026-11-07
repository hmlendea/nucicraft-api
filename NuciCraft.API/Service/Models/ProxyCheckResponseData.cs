using System.Text.Json.Serialization;

namespace NuciCraft.API.Service.Models
{
    public sealed class ProxyCheckResponseData
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("ip")]
        public ProxyCheckIpData IpAddress { get; set; }
    }
}
