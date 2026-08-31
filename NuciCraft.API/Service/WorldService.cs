using System;
using System.Collections.Generic;
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
    public sealed class WorldService(
        IFileRepository<WorldDataObject> repository,
        ILogger logger) : IWorldService
    {
        public void Add(AddWorldRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(
                MyOperation.AddWorld,
                OperationStatus.Started,
                logInfos);

            try
            {
                WorldDataObject worldDataObject = new()
                {
                    Id = request.Identifier,
                    Name = request.Name,
                    HasWebMap = request.HasWebMap,
                    SpawnPoint = request.SpawnPoint,
                    Type = WorldType.FromString(request.Type).ExternalName,
                    CreatedDT = TimestampFormats.GetCurrentUtcTimestamp()
                };

                repository.Add(worldDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.AddWorld,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.AddWorld,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public World GetWorld(string worldIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, worldIdentifier)
            ];

            logger.Info(
                MyOperation.GetWorld,
                OperationStatus.Started,
                logInfos);

            try
            {
                World world = repository.Get(worldIdentifier).ToServiceModel();

                logger.Info(
                    MyOperation.GetWorld,
                    OperationStatus.Success,
                    logInfos);

                return world;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetWorld,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public IEnumerable<World> GetAllWorlds()
        {
            logger.Info(
                MyOperation.GetAllWorlds,
                OperationStatus.Started);

            try
            {
                IEnumerable<World> worlds = repository.GetAll().ToServiceModels();

                logger.Info(
                    MyOperation.GetAllWorlds,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, worlds.Count()));

                return worlds;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetAllWorlds,
                    OperationStatus.Failure,
                    exception);

                throw;
            }
        }

        public void Update(PatchWorldRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(
                MyOperation.UpdateWorld,
                OperationStatus.Started,
                logInfos);

            try
            {
                ValidatePatchSelector(request);

                WorldDataObject worldDataObject = repository.Get(request.Identifier);

                ApplyPatchValues(request, worldDataObject);

                worldDataObject.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();

                repository.Update(worldDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateWorld,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.UpdateWorld,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static void ValidatePatchSelector(PatchWorldRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                throw new ArgumentException("The world identifier must be provided.");
            }
        }

        private static void ApplyPatchValues(
            PatchWorldRequest request,
            WorldDataObject worldDataObject)
        {
            if (request.Name is not null)
            {
                worldDataObject.Name = worldDataObject.Name.MergeWith(request.Name);
            }

            if (request.HasWebMap.HasValue)
            {
                worldDataObject.HasWebMap = request.HasWebMap.Value;
            }

            if (request.SpawnPoint is not null)
            {
                worldDataObject.SpawnPoint = request.SpawnPoint;
            }

            if (request.Type is not null)
            {
                worldDataObject.Type = WorldType.FromString(request.Type).ExternalName;
            }
        }
    }
}
