using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// Zone mapping extensions for converting between data objects and service models.
    /// </summary>
    static class ZoneMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a service model.
        /// </summary>
        /// <returns>The service model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static Zone ToServiceModel(this ZoneDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            Name = dataObject.Name?.ToServiceModel(),
            Nickname = dataObject.Nickname?.ToServiceModel(),
            Type = dataObject.Type,
            County = dataObject.County,
            Region = dataObject.Region,
            Country = dataObject.Country,
            World = dataObject.World,
            CreationDate = dataObject.CreationDate,
            Owners = dataObject.Owners,
            Creators = dataObject.Creators,
            Leaders = dataObject.Leaders,
            TeleportationPoint = dataObject.TeleportationPoint?.ToServiceModel(),
            Bounds = dataObject.Bounds?.ToServiceModel(),
            LeaderTitle = dataObject.LeaderTitle?.ToServiceModel(),
            Population = dataObject.Population,
            MapLink = dataObject.MapLink,
            WikiUrl = dataObject.WikiUrl
        };

        /// <summary>
        /// Converts the service model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="serviceModel">The service model.</param>
        internal static ZoneDataObject ToDataObject(this Zone serviceModel) => new()
        {
            Id = serviceModel.Identifier,
            Name = serviceModel.Name?.ToDataObject(),
            Nickname = serviceModel.Nickname?.ToDataObject(),
            Type = serviceModel.Type,
            County = serviceModel.County,
            Region = serviceModel.Region,
            Country = serviceModel.Country,
            World = serviceModel.World,
            CreationDate = serviceModel.CreationDate,
            Owners = serviceModel.Owners,
            Creators = serviceModel.Creators,
            Leaders = serviceModel.Leaders,
            TeleportationPoint = serviceModel.TeleportationPoint?.ToDataObject(),
            Bounds = serviceModel.Bounds?.ToDataObject(),
            LeaderTitle = serviceModel.LeaderTitle?.ToDataObject(),
            Population = serviceModel.Population,
            MapLink = serviceModel.MapLink,
            WikiUrl = serviceModel.WikiUrl
        };

        /// <summary>
        /// Converts the data objects into service models.
        /// </summary>
        /// <returns>The service models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<Zone> ToServiceModels(this IEnumerable<ZoneDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        /// <summary>
        /// Converts the service models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="serviceModels">The service models.</param>
        internal static IEnumerable<ZoneDataObject> ToDataObjects(this IEnumerable<Zone> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
