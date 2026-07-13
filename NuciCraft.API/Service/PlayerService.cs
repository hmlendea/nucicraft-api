using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public class PlayerService(
        IFileRepository<PlayerEntity> repository,
        ILogger logger) : IPlayerService
    {
        public void Register(RegisterPlayerRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID),
                new(MyLogInfoKey.CreatedDT, request.CreatedDT),
                new(MyLogInfoKey.IpAddress, request.IpAddress),
                new(MyLogInfoKey.SkinUrl, request.SkinUrl)
            ];

            logger.Info(
                MyOperation.RegisterPlayer,
                OperationStatus.Started,
                logInfos);

            try
            {
                Player player = new()
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Username = request.Username,
                    OfflineUUID = GetOfflineUuid(request.Username),
                    OnlineUUID = request.OnlineUUID,
                    CreatedDT = request.CreatedDT != null ? DateTimeOffset.Parse(request.CreatedDT) : DateTimeOffset.Now,
                    Password = request.Password,
                    IpAddress = request.IpAddress,
                    SkinUrl = request.SkinUrl
                };

                logInfos = logInfos
                    .Append(new(MyLogInfoKey.OfflineUUID, player.OfflineUUID))
                    .Append(new(MyLogInfoKey.CreatedDT, player.CreatedDT));

                repository.Add(player.ToDataObject());
                repository.SaveChanges();

                logger.Info(
                    MyOperation.RegisterPlayer,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.RegisterPlayer,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        public Player Get(GetPlayerRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.PlayerID, request.Identifier),
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OfflineUUID, request.OfflineUUID),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID)
            ];

            logger.Info(
                MyOperation.GetPlayer,
                OperationStatus.Started,
                logInfos);

            try
            {
                Player player = repository
                    .GetAll()
                    .FirstOrDefault(entity =>
                        (!string.IsNullOrWhiteSpace(request.Identifier) && entity.Id == request.Identifier) ||
                        (!string.IsNullOrWhiteSpace(request.Username) && entity.Username == request.Username) ||
                        (!string.IsNullOrWhiteSpace(request.OfflineUUID) && entity.OfflineUUID == request.OfflineUUID) ||
                        (!string.IsNullOrWhiteSpace(request.OnlineUUID) && entity.OnlineUUID == request.OnlineUUID))
                    ?.ToDomainModel()
                    ?? throw new KeyNotFoundException("No player found matching the provided criteria.");

                logger.Info(
                    MyOperation.GetPlayer,
                    OperationStatus.Success,
                    logInfos);

                return player;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GetPlayer,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        public void Update(UpdatePlayerRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.PlayerID, request.Identifier)
            ];

            logger.Info(
                MyOperation.UpdatePlayer,
                OperationStatus.Started,
                logInfos);

            try
            {
                PlayerEntity player = repository.Get(request.Identifier);

                if (request.Username is not null)
                    player.Username = request.Username;

                if (request.OnlineUUID is not null)
                    player.OnlineUUID = request.OnlineUUID;

                if (request.Password is not null)
                    player.Password = request.Password;

                if (request.IpAddress is not null)
                    player.IpAddress = request.IpAddress;

                if (request.DiscordId is not null)
                    player.DiscordId = request.DiscordId;

                if (request.EmailAddress is not null)
                    player.EmailAddress = request.EmailAddress;

                if (request.LastSleptDT is not null)
                    player.LastSleptDT = request.LastSleptDT;

                if (request.LastDeathDT is not null)
                    player.LastDeathDT = request.LastDeathDT;

                if (request.LastDeathLocation is not null)
                {
                    player.LastDeathLocation = new CoordinatesDataObject
                    {
                        World = request.LastDeathLocation.World,
                        X = request.LastDeathLocation.X,
                        Y = request.LastDeathLocation.Y,
                        Z = request.LastDeathLocation.Z
                    };
                }

                if (request.SkinUrl is not null)
                    player.SkinUrl = request.SkinUrl;

                player.UpdatedDT = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");

                repository.Update(player);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdatePlayer,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.UpdatePlayer,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        public void UpdateLastDeathLocation(string username, Coordinates location)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, username),
                new(MyLogInfoKey.DeathLocation, location)
            ];

            logger.Info(
                MyOperation.UpdateLastDeathLocation,
                OperationStatus.Started,
                logInfos);

            try
            {
                Player player = repository
                    .Get(username)
                    .ToDomainModel();

                player.UpdatedDT = DateTimeOffset.UtcNow;
                player.LastDeathDT = DateTimeOffset.UtcNow;
                player.LastDeathLocation = location;

                repository.Update(player.ToDataObject());
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdateLastDeathLocation,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.UpdateLastDeathLocation,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        static string GetOfflineUuid(string username)
        {
            string input = $"OfflinePlayer:{username}";

            // Compute MD5 hash
            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert to hex string
            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hexBuilder.Append(b.ToString("x2"));
            }

            string byteArray = hexBuilder.ToString();

            // Modify specific bytes (UUID v3 format adjustments)
            int byte6 = (Convert.ToInt32(byteArray.Substring(6, 2), 16) & 0x0f) | 0x30;
            int byte8 = (Convert.ToInt32(byteArray.Substring(8, 2), 16) & 0x3f) | 0x80;

            byteArray =
                byteArray[..6] +
                byte6.ToString("x2") +
                byte8.ToString("x2") +
                byteArray[10..];

            // Format as UUID
            return
                $"{byteArray[..8]}-" +
                $"{byteArray.Substring(8, 4)}-" +
                $"{byteArray.Substring(12, 4)}-" +
                $"{byteArray.Substring(16, 4)}-" +
                $"{byteArray[20..]}";
        }
    }
}