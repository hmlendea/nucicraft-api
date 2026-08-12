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
    public class PlayerService(
        IFileRepository<PlayerDataObject> repository,
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
                PlayerDataObject matchingDataObject = repository
                    .GetAll()
                    .FirstOrDefault(entity =>
                        (!string.IsNullOrWhiteSpace(request.Identifier) && string.Equals(entity.Id, request.Identifier)) ||
                        (!string.IsNullOrWhiteSpace(request.Username) && string.Equals(entity.Username, request.Username)) ||
                        (!string.IsNullOrWhiteSpace(request.OfflineUUID) && string.Equals(entity.OfflineUUID, request.OfflineUUID)) ||
                        (!string.IsNullOrWhiteSpace(request.OnlineUUID) && string.Equals(entity.OnlineUUID, request.OnlineUUID)));

                if (matchingDataObject is null)
                {
                    throw new KeyNotFoundException("No player found matching the provided criteria.");
                }

                Player player = matchingDataObject.ToDomainModel();

                logger.Info(
                    MyOperation.GetPlayer,
                    OperationStatus.Success,
                    logInfos);

                return player;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetPlayer,
                    OperationStatus.Failure,
                    exception,
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
                PlayerDataObject playerDataObject = repository.Get(request.Identifier);

                if (request.Username is not null)
                {
                    playerDataObject.Username = request.Username;
                }

                if (request.OnlineUUID is not null)
                {
                    playerDataObject.OnlineUUID = request.OnlineUUID;
                }

                if (request.Password is not null)
                {
                    playerDataObject.Password = request.Password;
                }

                if (request.IpAddress is not null)
                {
                    playerDataObject.IpAddress = request.IpAddress;
                }

                if (request.DiscordId is not null)
                {
                    playerDataObject.DiscordId = request.DiscordId;
                }

                if (request.EmailAddress is not null)
                {
                    playerDataObject.EmailAddress = request.EmailAddress;
                }

                if (request.LastSleptDT is not null)
                {
                    playerDataObject.LastSleptDT = request.LastSleptDT;
                }

                if (request.LastDeathDT is not null)
                {
                    playerDataObject.LastDeathDT = request.LastDeathDT;
                }

                if (request.LastDeathLocation is not null)
                {
                    playerDataObject.LastDeathLocation = request.LastDeathLocation;
                }

                if (request.BackLocation is not null)
                {
                    playerDataObject.BackLocation = request.BackLocation;
                }

                if (request.LogoutLocation is not null)
                {
                    playerDataObject.LogoutLocation = request.LogoutLocation;
                }

                if (request.SkinUrl is not null)
                {
                    playerDataObject.SkinUrl = request.SkinUrl;
                }

                if (request.Settings is not null)
                {
                    playerDataObject.Settings = MergePlayerSettingsDataObject(
                        playerDataObject.Settings,
                        request.Settings);
                }

                playerDataObject.UpdatedDT = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");

                repository.Update(playerDataObject);
                repository.SaveChanges();

                logger.Info(
                    MyOperation.UpdatePlayer,
                    OperationStatus.Success,
                    logInfos);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.UpdatePlayer,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static PlayerSettingsDataObject MergePlayerSettingsDataObject(
            PlayerSettingsDataObject existingSettings,
            PlayerSettingsDataObject incomingSettings)
        {
            if (existingSettings is null)
            {
                existingSettings = new PlayerSettingsDataObject();
            }

            if (incomingSettings.AutomaticHotbarRefillingIsEnabled is not null)
            {
                existingSettings.AutomaticHotbarRefillingIsEnabled = incomingSettings.AutomaticHotbarRefillingIsEnabled.Value;
            }

            if (incomingSettings.AutomaticSaplingReplantingIsEnabled is not null)
            {
                existingSettings.AutomaticSaplingReplantingIsEnabled = incomingSettings.AutomaticSaplingReplantingIsEnabled.Value;
            }

            if (incomingSettings.AutomaticToolSelectionIsEnabled is not null)
            {
                existingSettings.AutomaticToolSelectionIsEnabled = incomingSettings.AutomaticToolSelectionIsEnabled.Value;
            }

            if (incomingSettings.KeepExperienceIsEnabled is not null)
            {
                existingSettings.KeepExperienceIsEnabled = incomingSettings.KeepExperienceIsEnabled.Value;
            }

            if (incomingSettings.KeepInventoryIsEnabled is not null)
            {
                existingSettings.KeepInventoryIsEnabled = incomingSettings.KeepInventoryIsEnabled.Value;
            }

            if (incomingSettings.PrivateMessagesAreEnabled is not null)
            {
                existingSettings.PrivateMessagesAreEnabled = incomingSettings.PrivateMessagesAreEnabled.Value;
            }

            if (incomingSettings.PrivateMessagesInterceptionIsEnabled is not null)
            {
                existingSettings.PrivateMessagesInterceptionIsEnabled = incomingSettings.PrivateMessagesInterceptionIsEnabled.Value;
            }

            return existingSettings;
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

        private static string GetOfflineUuid(string username)
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