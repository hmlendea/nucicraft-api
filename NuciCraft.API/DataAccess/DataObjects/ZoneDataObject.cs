using System.Collections.Generic;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public class ZoneDataObject : NuciCraftEntityBase
    {
        public LocalisedStringDataObject Name { get; set; }

        public LocalisedStringDataObject Nickname { get; set; }

        public string Level { get; set; }

        public string County { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string CreationDate { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public IEnumerable<string> Creators { get; set; }

        public IEnumerable<string> Leaders { get; set; }

        public CoordinatesDataObject TeleportationPoint { get; set; }

        public LocalisedStringDataObject LeaderTitle { get; set; }

        public int Population { get; set; }

        public string MapLink { get; set; } // TODO: Fetch dynamically

        public string WikiUrl { get; set; } // TODO: Fetch dynamically
    }
}
