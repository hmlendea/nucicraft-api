using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// World mapping extensions for converting between data objects and service models.
    /// </summary>
    static class WorldMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static World ToServiceModel(this WorldDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            Name = dataObject.Name?.ToServiceModel()
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static WorldDataObject ToDataObject(this World serviceModel) => new()
        {
            Id = serviceModel.Identifier,
            Name = serviceModel.Name?.ToDataObject()
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<World> ToServiceModels(this IEnumerable<WorldDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());
    }
}
