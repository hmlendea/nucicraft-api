using System;
using System.Text.Json.Serialization;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetHomeResponse(Home home) : NuciApiResponseContent
    {
        [JsonPropertyName("id")]
        [HmacOrder(1)]
        public string Identifier { get; set; } = home.Identifier;

        [HmacOrder(2)]
        public DateTimeOffset CreatedDT { get; set; } = home.CreatedDT;

        [HmacOrder(3)]
        public DateTimeOffset? UpdatedDT { get; set; } = home.UpdatedDT;

        [HmacOrder(4)]
        public LocalisedString Name { get; set; } = home.Name;

        [HmacOrder(5)]
        public string Player { get; set; } = home.Player;

        [HmacOrder(6)]
        public Coordinates Location { get; set; } = home.Location;
    }
}