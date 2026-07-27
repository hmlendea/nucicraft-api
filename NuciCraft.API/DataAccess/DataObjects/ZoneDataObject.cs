using System.Collections.Generic;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public class ZoneDataObject : NuciCraftEntityBase
    {
        public string Name { get; set; } // TODO: Localisation support

        public string Nickname { get; set; } // TODO: Localisation support

        public string Type { get; set; }

        public string County { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string CreationDate { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public IEnumerable<string> Creators { get; set; }

        public IEnumerable<string> Leaders { get; set; }

        public CoordinatesDataObject TeleportationPoint { get; set; }

        public string LeaderTitle { get; set; } // TODO: Localisation support

        public int Population { get; set; }

        public string MapLink { get; set; } // TODO: Fetch dynamically

        public string WikiUrl { get; set; } // TODO: Fetch dynamically
    }
}
