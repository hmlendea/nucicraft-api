using System.Collections.Generic;
using System.Linq;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    /// <summary>
    /// RtpLocation mapping extensions for converting between data objects and domain models.
    /// </summary>
    static class RtpLocationMappingExtensions
    {
        /// <summary>
        /// Converts the data object into a domain model.
        /// </summary>
        /// <returns>The domain model.</returns>
        /// <param name="dataObject">The data object.</param>
        internal static RtpLocation ToDomainModel(this RtpLocationEntity dataObject) => new()
        {
            Id = dataObject.Id,
            Biome = dataObject.Biome,
            World = dataObject.World,
            X = dataObject.X,
            Y = dataObject.Y,
            Z = dataObject.Z
        };

        /// <summary>
        /// Converts the domain model into a data object.
        /// </summary>
        /// <returns>The data object.</returns>
        /// <param name="domainModel">The domain model.</param>
        internal static RtpLocationEntity ToDataObject(this RtpLocation domainModel) => new()
        {
            Id = domainModel.Id,
            Biome = domainModel.Biome,
            World = domainModel.World,
            X = domainModel.X,
            Y = domainModel.Y,
            Z = domainModel.Z
        };

        /// <summary>
        /// Converts the data objects into domain models.
        /// </summary>
        /// <returns>The domain models.</returns>
        /// <param name="dataObjects">The data objects.</param>
        internal static IEnumerable<RtpLocation> ToDomainModels(this IEnumerable<RtpLocationEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToDomainModel());

        /// <summary>
        /// Converts the domain models into data objects.
        /// </summary>
        /// <returns>The data objects.</returns>
        /// <param name="domainModels">The domain models.</param>
        internal static IEnumerable<RtpLocationEntity> ToDataObjects(this IEnumerable<RtpLocation> domainModels)
            => domainModels.Select(domainModel => domainModel.ToDataObject());
    }
}
