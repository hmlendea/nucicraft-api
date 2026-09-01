using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Helpers;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class ZoneService(
        IFileRepository<ZoneDataObject> repository,
        IFileRepository<WorldDataObject> worldRepository,
        IFileRepository<ZoneTypeDataObject> zoneTypeRepository,
        ILogger logger) : IZoneService
    {
        private static float BoundsPitch => 0f;

        private static float BoundsYaw => 0f;

        private static string RomaniaTimeZoneId => "Europe/Bucharest";

        public void Add(AddZoneRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidateWorldForAdd(request);
            ValidateBoundsForAdd(request.Bounds);
            ValidateTypeForAdd(request);

            ZoneBoundsDataObject normalisedBounds = GetNormalisedBounds(request.Bounds);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier),
            ];

            logger.Info(
                MyOperation.AddZone,
                OperationStatus.Started,
                logInfos);

            if (request.TeleportationPoint is not null)
            {
                logInfos = logInfos.Append(new(
                    MyLogInfoKey.World,
                    request.TeleportationPoint.World));
            }

            try
            {
                ZoneDataObject zoneDataObject = new()
                {
                    Id = request.Identifier,
                    Name = request.Name,
                    Nickname = request.Nickname,
                    Type = request.Type,
                    County = request.County,
                    Region = request.Region,
                    Country = request.Country,
                    World = request.World,
                    CreationDate = GetCreationDateForAddRequest(request),
                    Owners = request.Owners,
                    Creators = GetCreatorsForAddRequest(request),
                    Leaders = request.Leaders,
                    TeleportationPoint = request.TeleportationPoint,
                    Bounds = normalisedBounds,
                    LeaderTitle = request.LeaderTitle,
                    Population = request.Population,
                    MapLink = request.MapLink,
                    WikiUrl = request.WikiUrl,
                    CreatedDT = TimestampFormats.GetCurrentUtcTimestamp()
                };

                repository.Add(zoneDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.AddZone,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.AddZone,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public void Delete(string zoneIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, zoneIdentifier)
            ];

            logger.Info(
                MyOperation.DeleteZone,
                OperationStatus.Started,
                logInfos);

            try
            {
                repository.Remove(zoneIdentifier);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.DeleteZone,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.DeleteZone,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Zone GetZone(string zoneIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, zoneIdentifier)
            ];

            logger.Info(
                MyOperation.GetZone,
                OperationStatus.Started,
                logInfos);

            try
            {
                ZoneDataObject zoneDataObject = repository.Get(zoneIdentifier);

                zoneDataObject.Bounds = GetNormalisedBounds(zoneDataObject.Bounds);

                Zone zone = zoneDataObject.ToServiceModel();

                logger.Info(
                    MyOperation.GetZone,
                    OperationStatus.Success,
                    logInfos);

                return zone;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetZone,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public IEnumerable<Zone> GetAllZones()
        {
            logger.Info(
                MyOperation.GetAllZones,
                OperationStatus.Started);

            try
            {
                IEnumerable<ZoneDataObject> zoneDataObjects = repository.GetAll()
                    .Select(zoneDataObject => GetNormalisedZoneDataObject(zoneDataObject));
                IEnumerable<Zone> zones = zoneDataObjects.ToServiceModels();

                logger.Info(
                    MyOperation.GetAllZones,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, zones.Count()));

                return zones;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetAllZones,
                    OperationStatus.Failure,
                    exception);

                throw;
            }
        }

        public IEnumerable<string> GetZoneIdentifiersContainingCoordinates(CoordinatesDataObject coordinates)
        {
            ArgumentNullException.ThrowIfNull(coordinates);

            ValidateCoordinates(coordinates);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.World, coordinates.World),
                new(MyLogInfoKey.X, coordinates.X),
                new(MyLogInfoKey.Y, coordinates.Y),
                new(MyLogInfoKey.Z, coordinates.Z)
            ];

            logger.Info(
                MyOperation.GetZonesByCoordinates,
                OperationStatus.Started,
                logInfos);

            try
            {
                IEnumerable<string> zoneIdentifiers = repository.GetAll()
                    .Where(zoneDataObject => ContainsCoordinates(zoneDataObject.Bounds, coordinates))
                    .Select(zoneDataObject => zoneDataObject.Id);

                logger.Info(
                    MyOperation.GetZonesByCoordinates,
                    OperationStatus.Success,
                    logInfos.Append(new(MyLogInfoKey.Count, zoneIdentifiers.Count())));

                return zoneIdentifiers;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetZonesByCoordinates,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public void Update(PatchZoneRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(
                MyOperation.UpdateZone,
                OperationStatus.Started,
                logInfos);

            try
            {
                ValidatePatchSelector(request);
                ValidateWorldForPatch(request);
                ValidateTypeForPatch(request);

                ZoneDataObject zoneDataObject = repository.Get(request.Identifier);

                ZoneBoundsDataObject mergedBounds = GetMergedBounds(request, zoneDataObject);

                if (request.Bounds is not null)
                {
                    ValidateBounds(mergedBounds);
                    mergedBounds = GetNormalisedBounds(mergedBounds);
                }

                ApplyPatchValues(request, zoneDataObject);

                if (request.Bounds is not null)
                {
                    zoneDataObject.Bounds = mergedBounds;
                }

                zoneDataObject.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();

                repository.Update(zoneDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateZone,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.UpdateZone,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static void ValidatePatchSelector(PatchZoneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                throw new ArgumentException("Zone identifier must be provided.");
            }
        }

        private static void ValidateCoordinates(CoordinatesDataObject coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates.World))
            {
                throw new ArgumentException("The coordinate world must be provided.");
            }
        }

        private static bool ContainsCoordinates(
            ZoneBoundsDataObject bounds,
            CoordinatesDataObject coordinates)
        {
            ZoneBoundsDataObject normalisedBounds = GetNormalisedBounds(bounds);

            if (normalisedBounds?.FirstCorner is null || normalisedBounds.SecondCorner is null)
            {
                return false;
            }

            return string.Equals(
                    normalisedBounds.FirstCorner.World,
                    coordinates.World,
                    StringComparison.Ordinal)
                && coordinates.X >= normalisedBounds.FirstCorner.X
                && coordinates.X <= normalisedBounds.SecondCorner.X
                && coordinates.Y <= normalisedBounds.FirstCorner.Y
                && coordinates.Y >= normalisedBounds.SecondCorner.Y
                && coordinates.Z >= normalisedBounds.FirstCorner.Z
                && coordinates.Z <= normalisedBounds.SecondCorner.Z;
        }

        private static void ApplyPatchValues(
            PatchZoneRequest request,
            ZoneDataObject zoneDataObject)
        {
            if (request.Name is not null)
            {
                zoneDataObject.Name = zoneDataObject.Name.MergeWith(request.Name);
            }

            if (request.Nickname is not null)
            {
                zoneDataObject.Nickname = zoneDataObject.Nickname.MergeWith(request.Nickname);
            }

            if (request.Type is not null)
            {
                zoneDataObject.Type = request.Type;
            }

            if (request.County is not null)
            {
                zoneDataObject.County = request.County;
            }

            if (request.Region is not null)
            {
                zoneDataObject.Region = request.Region;
            }

            if (request.Country is not null)
            {
                zoneDataObject.Country = request.Country;
            }

            if (request.CreationDate is not null)
            {
                zoneDataObject.CreationDate = request.CreationDate;
            }

            if (request.World is not null)
            {
                zoneDataObject.World = request.World;
            }

            if (request.Owners is not null)
            {
                zoneDataObject.Owners = request.Owners;
            }

            if (request.Creators is not null)
            {
                zoneDataObject.Creators = request.Creators;
            }

            if (request.Leaders is not null)
            {
                zoneDataObject.Leaders = request.Leaders;
            }

            if (request.TeleportationPoint is not null)
            {
                zoneDataObject.TeleportationPoint = request.TeleportationPoint;
            }

            if (request.LeaderTitle is not null)
            {
                zoneDataObject.LeaderTitle = zoneDataObject.LeaderTitle.MergeWith(request.LeaderTitle);
            }

            if (request.Population is not null)
            {
                zoneDataObject.Population = request.Population.Value;
            }

            if (request.MapLink is not null)
            {
                zoneDataObject.MapLink = request.MapLink;
            }

            if (request.WikiUrl is not null)
            {
                zoneDataObject.WikiUrl = request.WikiUrl;
            }
        }

        private static IEnumerable<string> GetCreatorsForAddRequest(AddZoneRequest request)
        {
            if (request.Creators is not null)
            {
                return request.Creators;
            }

            if (request.Owners is null)
            {
                return null;
            }

            string[] owners = request.Owners.ToArray();

            if (owners.Length == 1)
            {
                return owners;
            }

            return null;
        }

        private static void ValidateWorldIdentifier(string worldIdentifier)
        {
            if (string.IsNullOrWhiteSpace(worldIdentifier))
            {
                throw new ArgumentException("The zone world identifier must be provided.");
            }
        }

        private void ValidateWorldForAdd(AddZoneRequest request)
        {
            ValidateWorldIdentifier(request.World);
            ValidateWorldExists(request.World);
        }

        private void ValidateWorldForPatch(PatchZoneRequest request)
        {
            if (request.World is null)
            {
                return;
            }

            ValidateWorldIdentifier(request.World);
            ValidateWorldExists(request.World);
        }

        private void ValidateWorldExists(string worldIdentifier)
        {
            try
            {
                WorldDataObject worldDataObject = worldRepository.Get(worldIdentifier);

                if (worldDataObject is null)
                {
                    throw new ArgumentException($"The zone world '{worldIdentifier}' is not valid.");
                }
            }
            catch (KeyNotFoundException exception)
            {
                throw new ArgumentException($"The zone world '{worldIdentifier}' is not valid.", exception);
            }
        }

        private void ValidateTypeForAdd(AddZoneRequest request)
        {
            ValidateTypeIdentifier(request.Type);
            ValidateTypeExists(request.Type);
        }

        private void ValidateTypeForPatch(PatchZoneRequest request)
        {
            if (request.Type is null)
            {
                return;
            }

            ValidateTypeIdentifier(request.Type);
            ValidateTypeExists(request.Type);
        }

        private static void ValidateTypeIdentifier(string zoneTypeIdentifier)
        {
            if (string.IsNullOrWhiteSpace(zoneTypeIdentifier))
            {
                throw new ArgumentException("The zone type identifier must be provided.");
            }
        }

        private void ValidateTypeExists(string zoneTypeIdentifier)
        {
            try
            {
                ZoneTypeDataObject zoneTypeDataObject = zoneTypeRepository.Get(zoneTypeIdentifier);

                if (zoneTypeDataObject is null)
                {
                    throw new ArgumentException($"The zone type '{zoneTypeIdentifier}' is not valid.");
                }
            }
            catch (KeyNotFoundException exception)
            {
                throw new ArgumentException($"The zone type '{zoneTypeIdentifier}' is not valid.", exception);
            }
        }

        private static string GetCreationDateForAddRequest(AddZoneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CreationDate))
            {
                return string.Concat(
                    GetRomaniaNow().ToString(
                        "yyyy'-'MM'-'dd",
                        CultureInfo.InvariantCulture),
                    " (?)");
            }

            return request.CreationDate;
        }

        private static ZoneBoundsDataObject GetMergedBounds(
            PatchZoneRequest request,
            ZoneDataObject zoneDataObject)
        {
            if (request.Bounds is null)
            {
                return zoneDataObject.Bounds;
            }

            ZoneBoundsDataObject existingBounds = zoneDataObject.Bounds;

            return MergeBounds(request.Bounds, existingBounds);
        }

        private static ZoneDataObject GetNormalisedZoneDataObject(ZoneDataObject zoneDataObject)
        {
            zoneDataObject.Bounds = GetNormalisedBounds(zoneDataObject.Bounds);

            return zoneDataObject;
        }

        private static ZoneBoundsDataObject GetNormalisedBounds(ZoneBoundsDataObject bounds)
        {
            if (bounds is null)
            {
                return null;
            }

            if (bounds.FirstCorner is null || bounds.SecondCorner is null)
            {
                return bounds;
            }

            string world = bounds.FirstCorner.World;

            return new ZoneBoundsDataObject
            {
                FirstCorner = new CoordinatesDataObject
                {
                    World = world,
                    X = Math.Min(bounds.FirstCorner.X, bounds.SecondCorner.X),
                    Y = Math.Max(bounds.FirstCorner.Y, bounds.SecondCorner.Y),
                    Z = Math.Min(bounds.FirstCorner.Z, bounds.SecondCorner.Z),
                    Pitch = BoundsPitch,
                    Yaw = BoundsYaw,
                },
                SecondCorner = new CoordinatesDataObject
                {
                    World = world,
                    X = Math.Max(bounds.FirstCorner.X, bounds.SecondCorner.X),
                    Y = Math.Min(bounds.FirstCorner.Y, bounds.SecondCorner.Y),
                    Z = Math.Max(bounds.FirstCorner.Z, bounds.SecondCorner.Z),
                    Pitch = BoundsPitch,
                    Yaw = BoundsYaw,
                },
            };
        }

        private static void ValidateBounds(ZoneBoundsDataObject bounds)
        {
            if (bounds is null)
            {
                return;
            }

            if (bounds.FirstCorner is null || bounds.SecondCorner is null)
            {
                throw new ArgumentException("Zone bounds must include both opposite corners.");
            }

            if (string.IsNullOrWhiteSpace(bounds.FirstCorner.World))
            {
                throw new ArgumentException("Zone bounds first corner world must be provided.");
            }

            if (string.IsNullOrWhiteSpace(bounds.SecondCorner.World))
            {
                throw new ArgumentException("Zone bounds second corner world must be provided.");
            }

            if (!string.Equals(bounds.FirstCorner.World, bounds.SecondCorner.World, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Zone bounds must be in the same world. First corner world: '{bounds.FirstCorner.World}'. Second corner world: '{bounds.SecondCorner.World}'.");
            }
        }

        private static void ValidateBoundsForAdd(ZoneBoundsDataObject bounds)
        {
            if (bounds is null)
            {
                throw new ArgumentException("The zone bounds must be provided.");
            }

            ValidateBounds(bounds);
        }

        private static ZoneBoundsDataObject MergeBounds(
            ZoneBoundsDataObject bounds,
            ZoneBoundsDataObject existingBounds)
        {
            if (existingBounds is null)
            {
                return bounds;
            }

            CoordinatesDataObject firstCorner = existingBounds.FirstCorner;

            if (bounds.FirstCorner is not null)
            {
                firstCorner = bounds.FirstCorner;
            }

            CoordinatesDataObject secondCorner = existingBounds.SecondCorner;

            if (bounds.SecondCorner is not null)
            {
                secondCorner = bounds.SecondCorner;
            }

            return new ZoneBoundsDataObject
            {
                FirstCorner = firstCorner,
                SecondCorner = secondCorner,
            };
        }

        private static DateTimeOffset GetRomaniaNow()
        {
            TimeZoneInfo romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RomaniaTimeZoneId);

            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, romaniaTimeZone);
        }
    }
}
