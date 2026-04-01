using System;
using System.Collections.Generic;
using System.Linq;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciExtensions;
using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public class RtpLocationService(
        IFileRepository<RtpLocationEntity> rtpLocationsRepository,
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
                RtpLocation rtpLocation = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Biome = request.Biome,
                    World = request.World,
                    X = request.X,
                    Y = request.Y,
                    Z = request.Z
                };

                rtpLocationsRepository.Add(rtpLocation.ToDataObject());
                rtpLocationsRepository.SaveChanges();

                logger.Info(
                    MyOperation.AddRtpLocation,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.AddRtpLocation,
                    OperationStatus.Failure,
                    ex,
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
                        .Where(location => request.World.Equals(location.World));
                }

                if (!string.IsNullOrWhiteSpace(request.Biome))
                {
                    rtpLocationEntities = rtpLocationEntities
                        .Where(location => request.Biome.Equals(location.Biome));
                }

                RtpLocation rtpLocation = rtpLocationEntities
                    .GetRandomElement()
                    .ToDomainModel();

                logInfos = logInfos
                    .Append(new(MyLogInfoKey.X, rtpLocation.X))
                    .Append(new(MyLogInfoKey.Y, rtpLocation.Y))
                    .Append(new(MyLogInfoKey.Z, rtpLocation.Z));

                logger.Info(
                    MyOperation.GetRandomRtpLocation,
                    OperationStatus.Success,
                    logInfos);

                return rtpLocation;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GetRandomRtpLocation,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }
    }
}