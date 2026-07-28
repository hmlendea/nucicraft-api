using System.Collections.Generic;
using System.Linq;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// RtpLocation mapping extensions for converting between data objects and service models.
    /// </summary>
    static class RtpLocationMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static RtpLocation ToServiceModel(
            this RtpLocationEntity dataObject) => new()
        {
            Id = dataObject.Id,
            Biome = dataObject.Biome,
            Coordinates = dataObject.Coordinates?.ToServiceModel()
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static RtpLocationEntity ToDataObject(
            this RtpLocation serviceModel) => new()
        {
            Id = serviceModel.Id,
            Biome = serviceModel.Biome,
            Coordinates = serviceModel.Coordinates?.ToDataObject()
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<RtpLocation> ToServiceModels(
            this IEnumerable<RtpLocationEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        /// <summary>
        /// Converts the service models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="serviceModels">The service models.</param>
        internal static IEnumerable<RtpLocationEntity> ToDataObjects(
            this IEnumerable<RtpLocation> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
