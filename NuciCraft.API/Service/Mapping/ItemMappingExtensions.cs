using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    static class ItemMappingExtensions
    {
        internal static Item ToServiceModel(this ItemDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            MinecraftId = dataObject.MinecraftId,
            BukkitId = dataObject.BukkitId,
            SignIds = dataObject.SignIds
        };

        internal static IEnumerable<Item> ToServiceModels(this IEnumerable<ItemDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());
    }
}