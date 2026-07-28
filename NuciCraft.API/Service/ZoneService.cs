using System;
using System.Collections.Generic;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;
using System.Linq;

namespace NuciCraft.API.Service
{
    public class ZoneService(
        IFileRepository<ZoneDataObject> repository,
        ILogger logger) : IZoneService
    {
        public Zone GetZone(string zoneIdentifier)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.ZoneIdentifier, zoneIdentifier)
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
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GetZone,
                    OperationStatus.Failure,
                    ex,
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
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GetAllZones,
                    OperationStatus.Failure,
                    ex);

                throw;
            }
        }
    }
}