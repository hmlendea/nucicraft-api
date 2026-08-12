using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciExtensions;
using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public sealed class RtpLocationService(
        IFileRepository<RtpLocationEntity> rtpLocationsRepository,
        RtpLocationSettings settings,
        ILogger logger) : IRtpLocationService
    {
        public void AddRtpLocation(AddRtpLocationRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Biome, request.Biome),
                new(MyLogInfoKey.World, request.World),
                new(MyLogInfoKey.X, request.X),
                new(MyLogInfoKey.Y, request.Y),
                new(MyLogInfoKey.Z, request.Z)
            ];

            logger.Info(
                MyOperation.AddRtpLocation,
                OperationStatus.Started,
                logInfos);

            try
            {
                if (!IsLocationFarAwayFromOtherLocations(request.World, request.X, request.Z))
                {
                    throw new ArgumentException("The provided location is too close to another existing location.");
                }

                if (!IsLocationFarAwayFromOtherLocationsInTheSameBiome(request.Biome, request.World, request.X, request.Z))
                {
                    throw new ArgumentException("The provided location is too close to another existing location in the same biome.");
                }

                rtpLocationsRepository.Add(new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Biome = request.Biome,
                    Coordinates = new()
                    {
                        World = request.World,
                        X = request.X,
                        Y = request.Y,
                        Z = request.Z
                    },
                    CreatedDT = DateTimeOffset.UtcNow.ToString(
                        TimestampFormats.Full,
                        CultureInfo.InvariantCulture)
                });

                rtpLocationsRepository.SaveChanges();

                logger.Info(
                    MyOperation.AddRtpLocation,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.AddRtpLocation,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public RtpLocation GetRtpLocation(GetRtpLocationRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Biome, request.Biome),
                new(MyLogInfoKey.World, request.World)
            ];

            logger.Info(
                MyOperation.GetRandomRtpLocation,
                OperationStatus.Started,
                logInfos);

            try
            {
                IEnumerable<RtpLocationEntity> rtpLocationEntities = rtpLocationsRepository.GetAll();

                if (!string.IsNullOrWhiteSpace(request.World))
                {
                    rtpLocationEntities = rtpLocationEntities
                        .Where(location => request.World.Equals(location.Coordinates.World));
                }

                if (!string.IsNullOrWhiteSpace(request.Biome))
                {
                    rtpLocationEntities = rtpLocationEntities
                        .Where(location => request.Biome.Equals(location.Biome));
                }

                RtpLocation rtpLocation = rtpLocationEntities
                    .GetRandomElement()
                    .ToServiceModel();

                logInfos = logInfos
                    .Append(new(MyLogInfoKey.X, rtpLocation.Coordinates?.X))
                    .Append(new(MyLogInfoKey.Y, rtpLocation.Coordinates?.Y))
                    .Append(new(MyLogInfoKey.Z, rtpLocation.Coordinates?.Z));

                logger.Info(
                    MyOperation.GetRandomRtpLocation,
                    OperationStatus.Success,
                    logInfos);

                return rtpLocation;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetRandomRtpLocation,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private bool IsLocationFarAwayFromOtherLocations(string world, float x, float z)
            => !rtpLocationsRepository
                .GetAll()
                .Any(location => AreLocationsTooClose(
                    world,
                    x,
                    z,
                    location.Coordinates.World,
                    location.Coordinates.X,
                    location.Coordinates.Z,
                    settings.MinimumLocationDistance));

        private bool IsLocationFarAwayFromOtherLocationsInTheSameBiome(
            string biome,
            string world,
            float x,
            float z)
            => !rtpLocationsRepository
                .GetAll()
                .Where(location => biome.Equals(location.Biome))
                .Any(location => AreLocationsTooClose(
                    world,
                    x,
                    z,
                    location.Coordinates.World,
                    location.Coordinates.X,
                    location.Coordinates.Z,
                    settings.MinimumBiomeLocationDistance));

        private static bool AreLocationsTooClose(
            string world1,
            float x1,
            float z1,
            string world2,
            float x2,
            float z2,
            int minimumDistance)
        {
            if (!string.Equals(world1, world2, StringComparison.Ordinal))
            {
                return false;
            }

            double deltaX = (double)x1 - x2;
            double deltaZ = (double)z1 - z2;

            return (deltaX * deltaX) + (deltaZ * deltaZ) <= (double)minimumDistance * minimumDistance;
        }
    }
}
