using System;
using System.Globalization;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mappings
{
    internal static class HomeMappingExtensions
    {
        internal static Home ToServiceModel(this HomeDataObject dataObject) => new()
        {
            Identifier = dataObject.Id,
            CreatedDT = DateTimeOffset.Parse(dataObject.CreatedDT, CultureInfo.InvariantCulture),
            UpdatedDT = ParseUpdatedTimestamp(dataObject.UpdatedDT),
            Name = dataObject.Name.ToServiceModel(),
            Player = dataObject.Player,
            Location = dataObject.Location.ToServiceModel()
        };

        internal static HomeDataObject ToDataObject(this Home home) => new()
        {
            Id = home.Identifier,
            CreatedDT = home.CreatedDT.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            UpdatedDT = home.UpdatedDT?.ToString(TimestampFormats.Full, CultureInfo.InvariantCulture),
            Name = home.Name.ToDataObject(),
            Player = home.Player,
            Location = home.Location.ToDataObject()
        };

        private static DateTimeOffset? ParseUpdatedTimestamp(string timestamp)
        {
            if (timestamp is null)
            {
                return null;
            }

            return DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
        }
    }
}