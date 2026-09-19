using System;
using System.Text.Json.Serialization;

namespace NuciCraft.API.Service.Models
{
    public sealed class Home
    {
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        public DateTimeOffset CreatedDT { get; set; }

        public DateTimeOffset? UpdatedDT { get; set; }

        public LocalisedString Name { get; set; }

        public string Player { get; set; }

        public Coordinates Location { get; set; }
    }
}