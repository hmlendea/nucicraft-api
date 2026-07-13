using System;
using System.Collections.Generic;
using System.Linq;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// Player mapping extensions for converting between data objects and domain models.
    /// </summary>
    static class PlayerMappingExtensions
    {
        static string TimestampFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

        /// <summary>
        /// Converts the data object into a domain model.
        /// </summary>
        /// <returns>The domain model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static Player ToDomainModel(this PlayerEntity dataObject) => new()
        {
            Username = dataObject.Id,
            OfflineUUID = dataObject.OfflineUUID,
            OnlineUUID = dataObject.OnlineUUID,
            Password = dataObject.Password,
            CreatedDT = DateTimeOffset.Parse(dataObject.CreatedDT),
            UpdatedDT = dataObject.UpdatedDT != null ? DateTimeOffset.Parse(dataObject.UpdatedDT) : null,
            IpAddress = dataObject.IpAddress,
            DiscordId = dataObject.DiscordId,
            SkinUrl = dataObject.SkinUrl
        };

        /// <summary>
        /// Converts the domain model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="domainModel">The domain model.</param>
        internal static PlayerEntity ToDataObject(this Player domainModel) => new()
        {
            Id = domainModel.Username,
            OfflineUUID = domainModel.OfflineUUID,
            OnlineUUID = domainModel.OnlineUUID,
            Password = domainModel.Password,
            CreatedDT = domainModel.CreatedDT.ToString(TimestampFormat),
            UpdatedDT = domainModel.UpdatedDT?.ToString(TimestampFormat),
            IpAddress = domainModel.IpAddress,
            DiscordId = domainModel.DiscordId,
            SkinUrl = domainModel.SkinUrl
        };

        /// <summary>
        /// Converts the data objects into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<Player> ToDomainModels(this IEnumerable<PlayerEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToDomainModel());

        /// <summary>
        /// Converts the domain models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="domainModels">The domain models.</param>
        internal static IEnumerable<PlayerEntity> ToDataObjects(this IEnumerable<Player> domainModels)
            => domainModels.Select(domainModel => domainModel.ToDataObject());
    }
}
