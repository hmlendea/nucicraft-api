using System.Collections.Generic;
using System.Text.Json.Serialization;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetZoneTypeResponse(ZoneType zoneType) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; } = zoneType.Identifier;

        [HmacOrder(2)]
        public IEnumerable<string> Categories { get; set; } = zoneType.Categories;

        [HmacOrder(3)]
        public LocalisedString Name { get; set; } = zoneType.Name;
    }
}