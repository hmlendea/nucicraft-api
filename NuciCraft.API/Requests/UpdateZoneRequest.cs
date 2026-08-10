using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class UpdateZoneRequest : NuciApiRequest
    {
        public string Identifier { get; set; }

        [HmacOrder(1)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Nickname { get; set; }

        [HmacOrder(3)]
        public string Level { get; set; }

        [HmacOrder(4)]
        public string County { get; set; }

        [HmacOrder(5)]
        public string Region { get; set; }

        [HmacOrder(6)]
        public string Country { get; set; }

        [HmacOrder(7)]
        public string CreationDate { get; set; }

        [HmacOrder(8)]
        public IEnumerable<string> Owners { get; set; }

        [HmacOrder(9)]
        public IEnumerable<string> Creators { get; set; }

        [HmacOrder(10)]
        public IEnumerable<string> Leaders { get; set; }

        [HmacOrder(11)]
        public CoordinatesDataObject TeleportationPoint { get; set; }

        [HmacOrder(12)]
        public LocalisedStringDataObject LeaderTitle { get; set; }

        [HmacOrder(13)]
        public int? Population { get; set; }

        [HmacOrder(14)]
        public string MapLink { get; set; }

        [HmacOrder(15)]
        public string WikiUrl { get; set; }
    }
}