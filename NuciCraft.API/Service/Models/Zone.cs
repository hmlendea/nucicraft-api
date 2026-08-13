using System.Collections.Generic;

namespace NuciCraft.API.Service.Models
{
    public class Zone
    {
        public string Identifier { get; set; }

        public LocalisedString Name { get; set; }

        public LocalisedString Nickname { get; set; }

        public string Level { get; set; }

        public string County { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string CreationDate { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public IEnumerable<string> Creators { get; set; }

        public IEnumerable<string> Leaders { get; set; }

        public Coordinates TeleportationPoint { get; set; }

        public ZoneBounds Bounds { get; set; }

        public LocalisedString LeaderTitle { get; set; }

        public int Population { get; set; }

        public string MapLink { get; set; } // TODO: Fetch dynamically

        public string WikiUrl { get; set; } // TODO: Fetch dynamically
    }
}
