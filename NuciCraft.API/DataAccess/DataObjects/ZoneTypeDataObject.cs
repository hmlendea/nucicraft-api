using System.Collections.Generic;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class ZoneTypeDataObject : NuciCraftEntityBase
    {
        public IEnumerable<string> Categories { get; set; }

        public LocalisedStringDataObject Name { get; set; }
    }
}