using System.Text.Json.Serialization;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public class LocalisedStringDataObject
    {
        [JsonPropertyName("default")]
        public string Default { get; set; }

        [JsonPropertyName("chinese")]
        public string Chinese { get; set; }

        [JsonPropertyName("dacian")]
        public string Dacian { get; set; }

        [JsonPropertyName("english")]
        public string English { get; set; }

        [JsonPropertyName("french")]
        public string French { get; set; }

        [JsonPropertyName("german")]
        public string German { get; set; }

        [JsonPropertyName("italian")]
        public string Italian { get; set; }

        [JsonPropertyName("japanese")]
        public string Japanese { get; set; }

        [JsonPropertyName("latin")]
        public string Latin { get; set; }

        [JsonPropertyName("nucian")]
        public string Nucian { get; set; }

        [JsonPropertyName("romanian")]
        public string Romanian { get; set; }
    }
}
