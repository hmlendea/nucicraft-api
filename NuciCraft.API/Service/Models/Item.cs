using System.Collections.Generic;

namespace NuciCraft.API.Service.Models
{
    public class Item
    {
        public string Identifier { get; set; }

        public string MinecraftId { get; set; }

        public string NuciCraftId { get; set; }

        public string BukkitId { get; set; }

        public List<string> SignIds { get; set; }
    }
}