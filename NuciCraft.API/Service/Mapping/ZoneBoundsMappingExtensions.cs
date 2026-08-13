using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// Zone bounds mapping extensions for converting between data objects and service models.
    /// </summary>
    internal static class ZoneBoundsMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static ZoneBounds ToServiceModel(this ZoneBoundsDataObject dataObject) => new()
        {
            FirstCorner = dataObject.FirstCorner?.ToServiceModel(),
            SecondCorner = dataObject.SecondCorner?.ToServiceModel(),
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static ZoneBoundsDataObject ToDataObject(this ZoneBounds serviceModel) => new()
        {
            FirstCorner = serviceModel.FirstCorner?.ToDataObject(),
            SecondCorner = serviceModel.SecondCorner?.ToDataObject(),
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<ZoneBounds> ToServiceModels(this IEnumerable<ZoneBoundsDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        /// <summary>
        /// Converts the service models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="serviceModels">The service models.</param>
        internal static IEnumerable<ZoneBoundsDataObject> ToDataObjects(this IEnumerable<ZoneBounds> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}