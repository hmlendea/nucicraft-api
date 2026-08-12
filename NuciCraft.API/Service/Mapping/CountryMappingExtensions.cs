using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// Country mapping extensions for converting between data objects and service models.
    /// </summary>
    static class CountryMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static Country ToServiceModel(this CountryDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            Name = dataObject.Name?.ToServiceModel(),
            LeaderTitle = dataObject.LeaderTitle?.ToServiceModel(),
            Leader = dataObject.Leader
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static CountryDataObject ToDataObject(this Country serviceModel) => new()
        {
            Id = serviceModel.Identifier,
            Name = serviceModel.Name?.ToDataObject(),
            LeaderTitle = serviceModel.LeaderTitle?.ToDataObject(),
            Leader = serviceModel.Leader
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<Country> ToServiceModels(this IEnumerable<CountryDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());
    }
}
