using System.Collections.Generic;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class ItemDataObject : NuciCraftEntityBase
    {
        public string MinecraftId { get; set; }

        public string NuciCraftId { get; set; }

        public string BukkitId { get; set; }

        public List<string> SignIds { get; set; }
    }
}