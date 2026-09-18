using System.Collections.Generic;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetZoneResponse(Zone zone) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Identifier { get; set; } = zone.Identifier;

        [HmacOrder(2)]
        public LocalisedString Name { get; set; } = zone.Name;

        [HmacOrder(3)]
        public LocalisedString Nickname { get; set; } = zone.Nickname;

        [HmacOrder(4)]
        public string Type { get; set; } = zone.Type;

        [HmacOrder(5)]
        public string County { get; set; } = zone.County;

        [HmacOrder(6)]
        public string Region { get; set; } = zone.Region;

        [HmacOrder(7)]
        public string Country { get; set; } = zone.Country;

        [HmacOrder(8)]
        public string World { get; set; } = zone.World;

        [HmacOrder(9)]
        public string CreationDate { get; set; } = zone.CreationDate;

        [HmacOrder(10)]
        public IEnumerable<string> Owners { get; set; } = zone.Owners;

        [HmacOrder(11)]
        public IEnumerable<string> Creators { get; set; } = zone.Creators;

        [HmacOrder(12)]
        public IEnumerable<string> Leaders { get; set; } = zone.Leaders;

        [HmacOrder(13)]
        public Coordinates TeleportationPoint { get; set; } = zone.TeleportationPoint;

        [HmacOrder(14)]
        public ZoneBounds Bounds { get; set; } = zone.Bounds;

        [HmacOrder(15)]
        public LocalisedString LeaderTitle { get; set; } = zone.LeaderTitle;

        [HmacOrder(16)]
        public int Population { get; set; } = zone.Population;

        [HmacOrder(17)]
        public string PopulationDate { get; set; } = zone.PopulationDate;

        [HmacOrder(18)]
        public string MapLink { get; set; } = zone.MapLink;

        [HmacOrder(19)]
        public string WikiUrl { get; set; } = zone.WikiUrl;
    }
}
