using System.Text.Json.Serialization;

namespace NuciCraft.API.Service.Models
{
    public sealed class GeolocationData
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("region")]
        public string RegionCode { get; set; } = string.Empty;

        [JsonPropertyName("regionName")]
        public string RegionName { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("zip")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("isp")]
        public string InternetServiceProvider { get; set; } = string.Empty;

        [JsonPropertyName("org")]
        public string Organisation { get; set; } = string.Empty;

        [JsonPropertyName("as")]
        public string AutonomousSystem { get; set; } = string.Empty;

        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;
    }
}