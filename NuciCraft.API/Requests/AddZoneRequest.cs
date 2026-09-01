using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class AddZoneRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(3)]
        public LocalisedStringDataObject Nickname { get; set; }

        [Required]
        [HmacOrder(4)]
        public string Type { get; set; }

        [HmacOrder(5)]
        public string County { get; set; }

        [HmacOrder(6)]
        public string Region { get; set; }

        [HmacOrder(7)]
        public string Country { get; set; }

        [Required]
        [HmacOrder(8)]
        public string World { get; set; }

        [HmacOrder(9)]
        public string CreationDate { get; set; }

        [HmacOrder(10)]
        public IEnumerable<string> Owners { get; set; }

        [HmacOrder(11)]
        public IEnumerable<string> Creators { get; set; }

        [HmacOrder(12)]
        public IEnumerable<string> Leaders { get; set; }

        [HmacOrder(13)]
        public CoordinatesDataObject TeleportationPoint { get; set; }

        [HmacOrder(14)]
        public LocalisedStringDataObject LeaderTitle { get; set; }

        [HmacOrder(15)]
        public int Population { get; set; }

        [HmacOrder(16)]
        public string PopulationDate { get; set; }

        [HmacOrder(17)]
        public string MapLink { get; set; }

        [HmacOrder(18)]
        public string WikiUrl { get; set; }

        [Required]
        [HmacOrder(19)]
        public ZoneBoundsDataObject Bounds { get; set; }
    }
}
