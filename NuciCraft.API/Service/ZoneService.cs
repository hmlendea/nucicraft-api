using System;
using System.Collections.Generic;
using System.Linq;

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
        public void Add(AddZoneRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.ZoneIdentifier, request.Identifier),
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
                    CreationDate = request.CreationDate,
                    Owners = request.Owners,
                    Creators = request.Creators,
                    Leaders = request.Leaders,
                    TeleportationPoint = request.TeleportationPoint,
                    LeaderTitle = request.LeaderTitle,
                    Population = request.Population,
                    MapLink = request.MapLink,
                    WikiUrl = request.WikiUrl,
                    CreatedDT = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")
                };

                repository.Add(zoneDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.AddZone,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.AddZone,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

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

        public void Update(UpdateZoneRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.ZoneIdentifier, request.Identifier)
            ];

            logger.Info(
                MyOperation.UpdateZone,
                OperationStatus.Started,
                logInfos);

            try
            {
                ZoneDataObject zoneDataObject = repository.Get(request.Identifier);

                if (request.Name is not null)
                {
                    zoneDataObject.Name = request.Name;
                }

                if (request.Nickname is not null)
                {
                    zoneDataObject.Nickname = request.Nickname;
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
                    zoneDataObject.LeaderTitle = request.LeaderTitle;
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

                zoneDataObject.UpdatedDT = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");

                repository.Update(zoneDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateZone,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.UpdateZone,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }
    }
}