using System.Collections.Generic;

namespace NuciCraft.API.Service.Models
{
    public sealed class Zone
    {
        private string customMapLink = null;

        public string Identifier { get; set; }

        public LocalisedString Name { get; set; }

        public LocalisedString Nickname { get; set; }

        public string Type { get; set; }

        public string County { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string World { get; set; }

        public string CreationDate { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public IEnumerable<string> Creators { get; set; }

        public IEnumerable<string> Leaders { get; set; }

        public Coordinates TeleportationPoint { get; set; }

        public ZoneBounds Bounds { get; set; }

        public LocalisedString LeaderTitle { get; set; }

        public int Population { get; set; }

        public string PopulationDate { get; set; }

        public string MapLink
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(customMapLink))
                {
                    return customMapLink;
                }

                if (TeleportationPoint is not null)
                {
                    return $"https://mc.nucilandia.ro/nucicraft/webmap/?worldname=world&x={TeleportationPoint.X}&z={TeleportationPoint.Z}";
                }

                return null!;
            }
            set
            {
                customMapLink = value;
            }
        }

        public string WikiUrl { get; set; } // TODO: Fetch dynamically.
    }
}
