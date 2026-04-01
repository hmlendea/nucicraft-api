using System;
using System.Collections.Generic;
using System.Linq;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// User mapping extensions for converting between data objects and domain models.
    /// </summary>
    static class UserMappingExtensions
    {
        static string TimestampFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

        /// <summary>
        /// Converts the data object into a domain model.
        /// </summary>
        /// <returns>The domain model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static User ToDomainModel(this UserEntity dataObject) => new()
        {
            Username = dataObject.Id,
            OfflineUUID = dataObject.OfflineUUID,
            OnlineUUID = dataObject.OnlineUUID,
            Password = dataObject.Password,
            CreatedDT = DateTimeOffset.Parse(dataObject.CreatedDT),
            UpdatedDT = dataObject.UpdatedDT != null ? DateTimeOffset.Parse(dataObject.UpdatedDT) : null,
            IpAddress = dataObject.IpAddress,
            SkinUrl = dataObject.SkinUrl
        };

        /// <summary>
        /// Converts the domain model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="domainModel">The domain model.</param>
        internal static UserEntity ToDataObject(this User domainModel) => new()
        {
            Id = domainModel.Username,
            OfflineUUID = domainModel.OfflineUUID,
            OnlineUUID = domainModel.OnlineUUID,
            Password = domainModel.Password,
            CreatedDT = domainModel.CreatedDT.ToString(TimestampFormat),
            UpdatedDT = domainModel.UpdatedDT?.ToString(TimestampFormat),
            IpAddress = domainModel.IpAddress,
            SkinUrl = domainModel.SkinUrl
        };

        /// <summary>
        /// Converts the data objects into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<User> ToDomainModels(this IEnumerable<UserEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToDomainModel());

        /// <summary>
        /// Converts the domain models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="domainModels">The domain models.</param>
        internal static IEnumerable<UserEntity> ToDataObjects(this IEnumerable<User> domainModels)
            => domainModels.Select(domainModel => domainModel.ToDataObject());
    }
}
