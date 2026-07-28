using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// Coordinates mapping extensions for converting between data objects and service models.
    /// </summary>
    static class CoordinatesMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static Coordinates ToServiceModel(this CoordinatesDataObject dataObject) => new()
        {
            World = dataObject.World,
            X = dataObject.X,
            Y = dataObject.Y,
            Z = dataObject.Z,
            Pitch = dataObject.Pitch,
            Yaw = dataObject.Yaw
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static CoordinatesDataObject ToDataObject(this Coordinates serviceModel) => new()
        {
            World = serviceModel.World,
            X = serviceModel.X,
            Y = serviceModel.Y,
            Z = serviceModel.Z,
            Pitch = serviceModel.Pitch,
            Yaw = serviceModel.Yaw
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<Coordinates> ToServiceModels(this IEnumerable<CoordinatesDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        /// <summary>
        /// Converts the service models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="serviceModels">The service models.</param>
        internal static IEnumerable<CoordinatesDataObject> ToDataObjects(this IEnumerable<Coordinates> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
