using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    static class ZoneTypeMappingExtensions
    {
        internal static ZoneType ToServiceModel(this ZoneTypeDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            Name = dataObject.Name?.ToServiceModel()
        };

        internal static IEnumerable<ZoneType> ToServiceModels(this IEnumerable<ZoneTypeDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());
    }
}