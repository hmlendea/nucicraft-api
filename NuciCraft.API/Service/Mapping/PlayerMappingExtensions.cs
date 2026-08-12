using System;
using System.Globalization;
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
        internal static Player ToDomainModel(this PlayerDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            Username = dataObject.Username,
            OfflineUUID = dataObject.OfflineUUID,
            OnlineUUID = dataObject.OnlineUUID,
            Password = dataObject.Password,
            CreatedDT = DateTimeOffset.Parse(dataObject.CreatedDT, CultureInfo.InvariantCulture),
            UpdatedDT = dataObject.UpdatedDT is not null ? DateTimeOffset.Parse(dataObject.UpdatedDT, CultureInfo.InvariantCulture) : null,
            IpAddress = dataObject.IpAddress,
            DiscordId = dataObject.DiscordId,
            EmailAddress = dataObject.EmailAddress,
            LastSleptDT = dataObject.LastSleptDT is not null ? DateTimeOffset.Parse(dataObject.LastSleptDT, CultureInfo.InvariantCulture) : null,
            LastDeathDT = dataObject.LastDeathDT is not null ? DateTimeOffset.Parse(dataObject.LastDeathDT, CultureInfo.InvariantCulture) : null,
            LastDeathLocation = dataObject.LastDeathLocation?.ToServiceModel(),
            BackLocation = dataObject.BackLocation?.ToServiceModel(),
            LogoutLocation = dataObject.LogoutLocation?.ToServiceModel(),
            Settings = dataObject.Settings.ToServiceModel()
        };

        /// <summary>
        /// Converts the domain model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="domainModel">The domain model.</param>
        internal static PlayerDataObject ToDataObject(this Player domainModel) => new()
        {
            Id = domainModel.Identifier,
            Username = domainModel.Username,
            OfflineUUID = domainModel.OfflineUUID,
            OnlineUUID = domainModel.OnlineUUID,
            Password = domainModel.Password,
            CreatedDT = domainModel.CreatedDT.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            UpdatedDT = domainModel.UpdatedDT?.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            IpAddress = domainModel.IpAddress,
            DiscordId = domainModel.DiscordId,
            EmailAddress = domainModel.EmailAddress,
            LastSleptDT = domainModel.LastSleptDT?.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            LastDeathDT = domainModel.LastDeathDT?.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            LastDeathLocation = domainModel.LastDeathLocation?.ToDataObject(),
            BackLocation = domainModel.BackLocation?.ToDataObject(),
            LogoutLocation = domainModel.LogoutLocation?.ToDataObject(),
            Settings = domainModel.Settings.ToDataObject()
        };

        /// <summary>
        /// Converts the data objects into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<Player> ToDomainModels(this IEnumerable<PlayerDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToDomainModel());

        /// <summary>
        /// Converts the domain models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="domainModels">The domain models.</param>
        internal static IEnumerable<PlayerDataObject> ToDataObjects(this IEnumerable<Player> domainModels)
            => domainModels.Select(domainModel => domainModel.ToDataObject());
    }
}
