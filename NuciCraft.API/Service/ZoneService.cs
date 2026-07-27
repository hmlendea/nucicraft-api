using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

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
    }
}