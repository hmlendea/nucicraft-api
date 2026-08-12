using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// LocalisedString mapping extensions for converting between data objects and service models.
    /// </summary>
    internal static class LocalisedStringMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static LocalisedString ToServiceModel(this LocalisedStringDataObject dataObject) => new()
        {
            Default = dataObject.Default,
            Chinese = dataObject.Chinese,
            Dacian = dataObject.Dacian,
            English = dataObject.English,
            French = dataObject.French,
            German = dataObject.German,
            Italian = dataObject.Italian,
            Japanese = dataObject.Japanese,
            Latin = dataObject.Latin,
            Nucian = dataObject.Nucian,
            Romanian = dataObject.Romanian
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static LocalisedStringDataObject ToDataObject(this LocalisedString serviceModel) => new()
        {
            Default = serviceModel.Default,
            Chinese = serviceModel.Chinese,
            Dacian = serviceModel.Dacian,
            English = serviceModel.English,
            French = serviceModel.French,
            German = serviceModel.German,
            Italian = serviceModel.Italian,
            Japanese = serviceModel.Japanese,
            Latin = serviceModel.Latin,
            Nucian = serviceModel.Nucian,
            Romanian = serviceModel.Romanian
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<LocalisedString> ToServiceModels(this IEnumerable<LocalisedStringDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        /// <summary>
        /// Converts the service models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="serviceModels">The service models.</param>
        internal static IEnumerable<LocalisedStringDataObject> ToDataObjects(this IEnumerable<LocalisedString> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
