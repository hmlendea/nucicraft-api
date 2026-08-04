using System.Collections.Generic;
using System.Text.Json.Serialization;

using NuciAPI.Responses;

namespace NuciCraft.API.Service
{
    public sealed class GenerateNamesResponse : NuciApiSuccessResponse
    {
        [JsonPropertyName("names")]
        public IEnumerable<string> Names { get; set; }
    }
}