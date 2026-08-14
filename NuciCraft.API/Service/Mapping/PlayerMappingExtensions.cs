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
    internal static class PlayerMappingExtensions
    {
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
            UpdatedDT = ParseNullableTimestamp(dataObject.UpdatedDT),
            LastIpAddress = dataObject.LastIpAddress,
            DiscordId = dataObject.DiscordId,
            EmailAddress = dataObject.EmailAddress,
            WikiUrl = dataObject.WikiUrl,
            IsBanned = dataObject.IsBanned,
            BannedDT = ParseNullableTimestamp(dataObject.BannedDT),
            IsMuted = dataObject.IsMuted,
            MutedDT = ParseNullableTimestamp(dataObject.MutedDT),
            LastLoginDT = ParseNullableTimestamp(dataObject.LastLoginDT),
            LastLogoutDT = ParseNullableTimestamp(dataObject.LastLogoutDT),
            LastLogoutLocation = dataObject.LastLogoutLocation?.ToServiceModel(),
            LastSleptDT = ParseNullableTimestamp(dataObject.LastSleptDT),
            BedLocation = dataObject.BedLocation?.ToServiceModel(),
            LastDeathDT = ParseNullableTimestamp(dataObject.LastDeathDT),
            LastDeathLocation = dataObject.LastDeathLocation?.ToServiceModel(),
            BackDT = ParseNullableTimestamp(dataObject.BackDT),
            BackLocation = dataObject.BackLocation?.ToServiceModel(),
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
            CreatedDT = domainModel.CreatedDT.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            UpdatedDT = domainModel.UpdatedDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            LastIpAddress = domainModel.LastIpAddress,
            DiscordId = domainModel.DiscordId,
            EmailAddress = domainModel.EmailAddress,
            WikiUrl = domainModel.WikiUrl,
            IsBanned = domainModel.IsBanned,
            BannedDT = domainModel.BannedDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            IsMuted = domainModel.IsMuted,
            MutedDT = domainModel.MutedDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            LastLoginDT = domainModel.LastLoginDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            LastLogoutDT = domainModel.LastLogoutDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            LastLogoutLocation = domainModel.LastLogoutLocation?.ToDataObject(),
            LastSleptDT = domainModel.LastSleptDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            BedLocation = domainModel.BedLocation?.ToDataObject(),
            LastDeathDT = domainModel.LastDeathDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            LastDeathLocation = domainModel.LastDeathLocation?.ToDataObject(),
            BackDT = domainModel.BackDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            BackLocation = domainModel.BackLocation?.ToDataObject(),
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

        private static DateTimeOffset? ParseNullableTimestamp(string timestamp)
        {
            if (timestamp is null)
            {
                return null;
            }

            return DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
        }
    }
}
