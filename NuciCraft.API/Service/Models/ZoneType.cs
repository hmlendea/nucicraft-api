using System.Collections.Generic;

namespace NuciCraft.API.Service.Models
{
    public sealed class ZoneType
    {
        public string Identifier { get; set; }

        public IEnumerable<string> Categories { get; set; }

        public LocalisedString Name { get; set; }
    }
}