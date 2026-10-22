using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Generators;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class ItemService(
        IFileRepository<ItemDataObject> repository,
        ILogger logger,
        ISignIdGenerator signIdGenerator) : IItemService
    {
        public void Add(AddItemRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidateRequest(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier),
                new(MyLogInfoKey.MinecraftId, request.MinecraftId),
                new(MyLogInfoKey.BukkitId, request.BukkitId)
            ];

            logger.Info(
                MyOperation.AddItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                string identifier = request.Identifier;

                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    identifier = Guid.NewGuid().ToString();
                }

                string nuciCraftId = request.NuciCraftId;
                string signIdSource = request.NuciCraftId;

                if (string.IsNullOrWhiteSpace(request.NuciCraftId))
                {
                    nuciCraftId = request.MinecraftId;
                    signIdSource = request.MinecraftId;
                }

                ItemDataObject itemDataObject = new()
                {
                    Id = identifier,
                    MinecraftId = request.MinecraftId.ToLowerInvariant(),
                    NuciCraftId = nuciCraftId.ToLowerInvariant(),
                    BukkitId = request.BukkitId.ToUpperInvariant(),
                    SignIds = request.SignIds ?? signIdGenerator.GenerateDefaultSignIds(signIdSource)
                };

                repository.Add(itemDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.AddItem,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.AddItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Item Get(string itemIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, itemIdentifier)
            ];

            logger.Info(
                MyOperation.GetItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                Item item = repository.Get(itemIdentifier).ToServiceModel();

                logger.Info(
                    MyOperation.GetItem,
                    OperationStatus.Success,
                    logInfos);

                return item;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Item GetByMinecraftId(string minecraftId)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.MinecraftId, minecraftId)
            ];

            logger.Info(
                MyOperation.GetItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                ItemDataObject itemDataObject = repository.GetAll()
                    .FirstOrDefault(item => string.Equals(item.MinecraftId, minecraftId, StringComparison.OrdinalIgnoreCase));

                if (itemDataObject is null)
                {
                    throw new KeyNotFoundException($"No item found with Minecraft ID '{minecraftId}'.");
                }

                Item item = itemDataObject.ToServiceModel();

                logger.Info(
                    MyOperation.GetItem,
                    OperationStatus.Success,
                    logInfos);

                return item;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Item GetByBukkitId(string bukkitId)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.BukkitId, bukkitId)
            ];

            logger.Info(
                MyOperation.GetItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                ItemDataObject itemDataObject = repository.GetAll()
                    .FirstOrDefault(item => string.Equals(item.BukkitId, bukkitId, StringComparison.OrdinalIgnoreCase));

                if (itemDataObject is null)
                {
                    throw new KeyNotFoundException($"No item found with Bukkit ID '{bukkitId}'.");
                }

                Item item = itemDataObject.ToServiceModel();

                logger.Info(
                    MyOperation.GetItem,
                    OperationStatus.Success,
                    logInfos);

                return item;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Item GetByNuciCraftId(string nuciCraftId)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.NuciCraftId, nuciCraftId)
            ];

            logger.Info(
                MyOperation.GetItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                ItemDataObject itemDataObject = repository.GetAll()
                    .FirstOrDefault(item => string.Equals(item.NuciCraftId, nuciCraftId, StringComparison.OrdinalIgnoreCase));

                if (itemDataObject is null)
                {
                    throw new KeyNotFoundException($"No item found with NuciCraft ID '{nuciCraftId}'.");
                }

                Item item = itemDataObject.ToServiceModel();

                logger.Info(
                    MyOperation.GetItem,
                    OperationStatus.Success,
                    logInfos);

                return item;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public IEnumerable<Item> GetAll()
        {
            logger.Info(
                MyOperation.GetAllItems,
                OperationStatus.Started);

            try
            {
                IEnumerable<Item> items = repository.GetAll().ToServiceModels();

                logger.Info(
                    MyOperation.GetAllItems,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, items.Count()));

                return items;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetAllItems,
                    OperationStatus.Failure,
                    exception);

                throw;
            }
        }

        public void Update(PatchItemRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidatePatchSelector(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(
                MyOperation.UpdateItem,
                OperationStatus.Started,
                logInfos);

            try
            {
                ItemDataObject itemDataObject = repository.Get(request.Identifier);

                ApplyPatchValues(request, itemDataObject);

                itemDataObject.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();

                repository.Update(itemDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateItem,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.UpdateItem,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static void ValidateRequest(AddItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MinecraftId))
            {
                throw new ArgumentException("The Minecraft ID must be provided.");
            }

            if (string.IsNullOrWhiteSpace(request.BukkitId))
            {
                throw new ArgumentException("The Bukkit ID must be provided.");
            }
        }

        private static void ValidatePatchSelector(PatchItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                throw new ArgumentException("The item identifier must be provided.");
            }
        }

        private static void ApplyPatchValues(
            PatchItemRequest request,
            ItemDataObject itemDataObject)
        {
            if (request.MinecraftId is not null)
            {
                itemDataObject.MinecraftId = request.MinecraftId.ToLowerInvariant();
            }

            if (request.BukkitId is not null)
            {
                itemDataObject.BukkitId = request.BukkitId.ToUpperInvariant();
            }

            if (request.SignIds is not null)
            {
                itemDataObject.SignIds = request.SignIds;
            }
        }
    }
}