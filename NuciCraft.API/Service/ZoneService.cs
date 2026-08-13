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
        ILogger logger) : IZoneService
    {
        private static string RomaniaTimeZoneId => "Europe/Bucharest";

        public void Add(AddZoneRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidateBounds(request.Bounds);

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
                    Level = request.Level,
                    County = request.County,
                    Region = request.Region,
                    Country = request.Country,
                    CreationDate = GetCreationDateForAddRequest(request),
                    Owners = request.Owners,
                    Creators = GetCreatorsForAddRequest(request),
                    Leaders = request.Leaders,
                    TeleportationPoint = request.TeleportationPoint,
                    Bounds = request.Bounds,
                    LeaderTitle = request.LeaderTitle,
                    Population = request.Population,
                    MapLink = request.MapLink,
                    WikiUrl = request.WikiUrl,
                    CreatedDT = DateTimeOffset.UtcNow.ToString(
                        TimestampFormats.Full,
                        CultureInfo.InvariantCulture)
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
                Zone zone = repository.Get(zoneIdentifier).ToServiceModel();

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
                IEnumerable<Zone> zones = repository.GetAll().ToServiceModels();

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

                ZoneDataObject zoneDataObject = repository.Get(request.Identifier);

                ZoneBoundsDataObject mergedBounds = GetMergedBounds(request, zoneDataObject);

                if (request.Bounds is not null)
                {
                    ValidateBounds(mergedBounds);
                }

                ApplyPatchValues(request, zoneDataObject);

                if (request.Bounds is not null)
                {
                    zoneDataObject.Bounds = mergedBounds;
                }

                zoneDataObject.UpdatedDT = DateTimeOffset.UtcNow.ToString(
                    TimestampFormats.Full,
                    CultureInfo.InvariantCulture);

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

            if (request.Level is not null)
            {
                zoneDataObject.Level = request.Level;
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