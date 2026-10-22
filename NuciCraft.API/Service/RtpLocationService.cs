using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;

using NuciExtensions;

using NuciLog.Core;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

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
                    CreatedDT = TimestampFormats.GetCurrentUtcTimestamp()
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
                        .Where(location => string.Equals(request.World, location.Coordinates.World, StringComparison.Ordinal));
                }

                if (!string.IsNullOrWhiteSpace(request.Biome))
                {
                    rtpLocationEntities = rtpLocationEntities
                        .Where(location => string.Equals(request.Biome, location.Biome, StringComparison.Ordinal));
                }

                if (!rtpLocationEntities.Any())
                {
                    throw new KeyNotFoundException("No RTP location found matching the provided criteria.");
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

        private bool IsLocationFarAwayFromOtherLocations(string world, float coordinateX, float coordinateZ)
            => IsLocationFarAwayFromLocations(
                rtpLocationsRepository.GetAll(),
                world,
                coordinateX,
                coordinateZ,
                settings.MinimumLocationDistance);

        private bool IsLocationFarAwayFromOtherLocationsInTheSameBiome(
            string biome,
            string world,
            float coordinateX,
            float coordinateZ)
            => IsLocationFarAwayFromLocations(
                rtpLocationsRepository
                    .GetAll()
                    .Where(location => string.Equals(biome, location.Biome, StringComparison.Ordinal)),
                world,
                coordinateX,
                coordinateZ,
                settings.MinimumBiomeLocationDistance);

        private static bool IsLocationFarAwayFromLocations(
            IEnumerable<RtpLocationEntity> locationEntities,
            string world,
            float coordinateX,
            float coordinateZ,
            int minimumDistance)
            => !locationEntities.Any(location => AreLocationsTooClose(
                    world,
                    coordinateX,
                    coordinateZ,
                    location.Coordinates.World,
                    location.Coordinates.X,
                    location.Coordinates.Z,
                    minimumDistance));

        private static bool AreLocationsTooClose(
            string firstWorld,
            float firstCoordinateX,
            float firstCoordinateZ,
            string secondWorld,
            float secondCoordinateX,
            float secondCoordinateZ,
            int minimumDistance)
        {
            if (!string.Equals(firstWorld, secondWorld, StringComparison.Ordinal))
            {
                return false;
            }

            double deltaX = (double)firstCoordinateX - secondCoordinateX;
            double deltaZ = (double)firstCoordinateZ - secondCoordinateZ;

            return (deltaX * deltaX) + (deltaZ * deltaZ) <= (double)minimumDistance * minimumDistance;
        }
    }
}
