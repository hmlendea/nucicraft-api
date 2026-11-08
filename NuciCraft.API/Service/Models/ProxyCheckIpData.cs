using System;
using System.Text.Json.Serialization;

namespace NuciCraft.API.Service.Models
{
    public sealed class ProxyCheckIpData
    {
        [JsonPropertyName("proxy")]
        public string ProxyValue { get; set; }

        [JsonIgnore]
        public bool IsProxy => string.Equals(ProxyValue, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
