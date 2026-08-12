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
                if (!IsLocationFarAwayFromOtherLocations(request.X, request.Y))
                {
                    throw new ArgumentException("The provided location is too close to another existing location.");
                }

                if (!IsLocationFarAwayFromOtherLocationsInTheSameBiome(request.Biome, request.X, request.Y))
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

        private bool IsLocationFarAwayFromOtherLocations(float x, float y)
            => !rtpLocationsRepository
                .GetAll()
                .Any(location => AreLocationsTooClose(x, y, location.Coordinates.X, location.Coordinates.Y, settings.MinimumLocationDistance));

        private bool IsLocationFarAwayFromOtherLocationsInTheSameBiome(string biome, float x, float y)
            => !rtpLocationsRepository
                .GetAll()
                .Where(location => biome.Equals(location.Biome))
                .Any(location => AreLocationsTooClose(x, y, location.Coordinates.X, location.Coordinates.Y, settings.MinimumBiomeLocationDistance));

        private static bool AreLocationsTooClose(
            float x1, float y1,
            float x2, float y2,
            int minimumDistance)
        {
            double deltaX = (double)x1 - x2;
            double deltaY = (double)y1 - y2;

            return (deltaX * deltaX) + (deltaY * deltaY) <= (double)minimumDistance * minimumDistance;
        }
    }
}