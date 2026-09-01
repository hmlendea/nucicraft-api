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
    public sealed class ZoneTypeService(
        IFileRepository<ZoneTypeDataObject> repository,
        ILogger logger) : IZoneTypeService
    {
        public void Add(AddZoneTypeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(MyOperation.AddZoneType, OperationStatus.Started, logInfos);

            try
            {
                ZoneTypeDataObject zoneTypeDataObject = new()
                {
                    Id = request.Identifier,
                    Name = request.Name,
                    CreatedDT = TimestampFormats.GetCurrentUtcTimestamp()
                };

                repository.Add(zoneTypeDataObject);
                repository.SaveChanges();

                logger.Info(MyOperation.AddZoneType, OperationStatus.Success, logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(MyOperation.AddZoneType, OperationStatus.Failure, exception, logInfos);

                throw;
            }
        }

        public ZoneType GetZoneType(string zoneTypeIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, zoneTypeIdentifier)
            ];

            logger.Info(MyOperation.GetZoneType, OperationStatus.Started, logInfos);

            try
            {
                ZoneType zoneType = repository.Get(zoneTypeIdentifier).ToServiceModel();

                logger.Info(MyOperation.GetZoneType, OperationStatus.Success, logInfos);

                return zoneType;
            }
            catch (Exception exception)
            {
                logger.Error(MyOperation.GetZoneType, OperationStatus.Failure, exception, logInfos);

                throw;
            }
        }

        public IEnumerable<ZoneType> GetAllZoneTypes()
        {
            logger.Info(MyOperation.GetAllZoneTypes, OperationStatus.Started);

            try
            {
                IEnumerable<ZoneType> zoneTypes = repository.GetAll().ToServiceModels();

                logger.Info(
                    MyOperation.GetAllZoneTypes,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, zoneTypes.Count()));

                return zoneTypes;
            }
            catch (Exception exception)
            {
                logger.Error(MyOperation.GetAllZoneTypes, OperationStatus.Failure, exception);

                throw;
            }
        }

        public void Update(PatchZoneTypeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidatePatchSelector(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Identifier, request.Identifier)
            ];

            logger.Info(MyOperation.UpdateZoneType, OperationStatus.Started, logInfos);

            try
            {
                ZoneTypeDataObject zoneTypeDataObject = repository.Get(request.Identifier);

                if (request.Name is not null)
                {
                    zoneTypeDataObject.Name = zoneTypeDataObject.Name.MergeWith(request.Name);
                }

                zoneTypeDataObject.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();

                repository.Update(zoneTypeDataObject);
                repository.SaveChanges();

                logger.Info(MyOperation.UpdateZoneType, OperationStatus.Success, logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(MyOperation.UpdateZoneType, OperationStatus.Failure, exception, logInfos);

                throw;
            }
        }

        private static void ValidatePatchSelector(PatchZoneTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                throw new ArgumentException("The zone type identifier must be provided.");
            }
        }
    }
}