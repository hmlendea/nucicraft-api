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
        internal static Player ToDomainModel(this PlayerEntity dataObject) => new()
        {
            Identifier = dataObject.Id,
            Username = dataObject.Username,
            OfflineUUID = dataObject.OfflineUUID,
            OnlineUUID = dataObject.OnlineUUID,
            Password = dataObject.Password,
            CreatedDT = ParseTimestamp(dataObject.CreatedDT),
            UpdatedDT = ParseNullableTimestamp(dataObject.UpdatedDT),
            IpAddress = dataObject.IpAddress,
            DiscordId = dataObject.DiscordId,
            EmailAddress = dataObject.EmailAddress,
            LastSleptDT = ParseNullableTimestamp(dataObject.LastSleptDT),
            LastDeathDT = ParseNullableTimestamp(dataObject.LastDeathDT),
            LastDeathLocation = ToServiceModel(dataObject.LastDeathLocation),
            BackLocation = ToServiceModel(dataObject.BackLocation),
            SkinUrl = dataObject.SkinUrl
        };

        /// <summary>
        /// Converts the domain model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="domainModel">The domain model.</param>
        internal static PlayerEntity ToDataObject(this Player domainModel) => new()
        {
            Id = domainModel.Identifier,
            Username = domainModel.Username,
            OfflineUUID = domainModel.OfflineUUID,
            OnlineUUID = domainModel.OnlineUUID,
            Password = domainModel.Password,
            CreatedDT = domainModel.CreatedDT.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            UpdatedDT = ToTimestamp(domainModel.UpdatedDT),
            IpAddress = domainModel.IpAddress,
            DiscordId = domainModel.DiscordId,
            EmailAddress = domainModel.EmailAddress,
            LastSleptDT = ToTimestamp(domainModel.LastSleptDT),
            LastDeathDT = ToTimestamp(domainModel.LastDeathDT),
            LastDeathLocation = ToDataObject(domainModel.LastDeathLocation),
            BackLocation = ToDataObject(domainModel.BackLocation),
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

        private static DateTimeOffset ParseTimestamp(string timestamp)
            => DateTimeOffset.ParseExact(timestamp, TimestampFormat, CultureInfo.InvariantCulture);

        private static DateTimeOffset? ParseNullableTimestamp(string timestamp)
        {
            if (timestamp is null)
            {
                return null;
            }

            return ParseTimestamp(timestamp);
        }

        private static string ToTimestamp(DateTimeOffset? timestamp)
        {
            if (timestamp is null)
            {
                return null;
            }

            return timestamp.Value.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        }

        private static Coordinates ToServiceModel(CoordinatesDataObject dataObject)
        {
            if (dataObject is null)
            {
                return null;
            }

            return dataObject.ToServiceModel();
        }

        private static CoordinatesDataObject ToDataObject(Coordinates serviceModel)
        {
            if (serviceModel is null)
            {
                return null;
            }

            return serviceModel.ToDataObject();
        }
    }
}
