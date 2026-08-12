using System;
using System.Collections.Generic;
using System.Globalization;
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
    public sealed class PlayerService(
        IFileRepository<PlayerDataObject> repository,
        ILogger logger) : IPlayerService
    {
        public void Register(RegisterPlayerRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID),
                new(MyLogInfoKey.CreatedDT, request.CreatedDT),
                new(MyLogInfoKey.IpAddress, request.IpAddress)
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
                    CreatedDT = GetCreatedDateTimeForRegisterRequest(request),
                    Password = request.Password,
                    IpAddress = request.IpAddress,
                    Settings = new PlayerSettings(),
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
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.RegisterPlayer,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Player Get(GetPlayerRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

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
                Func<PlayerDataObject, bool> matchesRequest = BuildPlayerDataObjectMatcher(
                    request.Identifier,
                    request.Username,
                    request.OfflineUUID,
                    request.OnlineUUID);

                PlayerDataObject matchingDataObject = repository
                    .GetAll()
                    .FirstOrDefault(matchesRequest);

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

        public IEnumerable<Player> GetAll()
        {
            logger.Info(
                MyOperation.GetAllPlayers,
                OperationStatus.Started);

            try
            {
                IEnumerable<Player> players = repository.GetAll().ToDomainModels();

                logger.Info(
                    MyOperation.GetAllPlayers,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.Count, players.Count()));

                return players;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetAllPlayers,
                    OperationStatus.Failure,
                    exception);

                throw;
            }
        }

        public void Update(PatchPlayerRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.PlayerID, request.Identifier),
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OfflineUUID, request.OfflineUUID),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID)
            ];

            logger.Info(
                MyOperation.UpdatePlayer,
                OperationStatus.Started,
                logInfos);

            try
            {
                ValidatePatchSelectors(request);

                PlayerDataObject playerDataObject = FindPlayerToPatch(request);

                ApplyPatchValues(request, playerDataObject);

                playerDataObject.UpdatedDT = DateTimeOffset.UtcNow.ToString(
                    TimestampFormats.Full,
                    CultureInfo.InvariantCulture);

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

        private static void ValidatePatchSelectors(PatchPlayerRequest request)
        {
            string[] selectors =
            [
                request.Identifier,
                request.Username,
                request.OfflineUUID,
                request.OnlineUUID
            ];

            int providedSelectorCount = selectors
                .Count(selectorValue => !string.IsNullOrWhiteSpace(selectorValue));

            if (providedSelectorCount != 1)
            {
                throw new ArgumentException("Exactly one player identifier selector must be provided.");
            }
        }

        private static Func<PlayerDataObject, bool> BuildPlayerDataObjectMatcher(
            string playerIdentifier,
            string playerUsername,
            string playerOfflineUuid,
            string playerOnlineUuid)
            => playerDataObject =>
                (!string.IsNullOrWhiteSpace(playerIdentifier) && string.Equals(playerDataObject.Id, playerIdentifier)) ||
                (!string.IsNullOrWhiteSpace(playerUsername) && string.Equals(playerDataObject.Username, playerUsername)) ||
                (!string.IsNullOrWhiteSpace(playerOfflineUuid) && string.Equals(playerDataObject.OfflineUUID, playerOfflineUuid)) ||
                (!string.IsNullOrWhiteSpace(playerOnlineUuid) && string.Equals(playerDataObject.OnlineUUID, playerOnlineUuid));

        private PlayerDataObject FindPlayerToPatch(PatchPlayerRequest request)
        {
            Func<PlayerDataObject, bool> matchesRequest = BuildPlayerDataObjectMatcher(
                request.Identifier,
                request.Username,
                request.OfflineUUID,
                request.OnlineUUID);

            return repository
                .GetAll()
                .FirstOrDefault(matchesRequest)
                ?? throw new KeyNotFoundException("No player found matching the provided criteria.");
        }

        private static void ApplyPatchValues(
            PatchPlayerRequest request,
            PlayerDataObject playerDataObject)
        {
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

            if (request.Settings is not null)
            {
                playerDataObject.Settings = MergePlayerSettingsDataObject(
                    playerDataObject.Settings,
                    request.Settings);
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

            if (incomingSettings.Localisation is not null)
            {
                existingSettings.Localisation = incomingSettings.Localisation;
            }

            if (incomingSettings.SkinUrl is not null)
            {
                existingSettings.SkinUrl = incomingSettings.SkinUrl;
            }

            return existingSettings;
        }

        private static DateTimeOffset GetCreatedDateTimeForRegisterRequest(RegisterPlayerRequest request)
        {
            if (request.CreatedDT is null)
            {
                return DateTimeOffset.UtcNow;
            }

            if (DateTimeOffset.TryParseExact(
                    request.CreatedDT,
                    TimestampFormats.Full,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTimeOffset createdDateTimeOffset))
            {
                return createdDateTimeOffset;
            }

            throw new ArgumentException(
                $"The created timestamp must match format '{TimestampFormats.Full}'.");
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
            int byte6 = (Convert.ToInt32(byteArray.Substring(12, 2), 16) & 0x0f) | 0x30;
            int byte8 = (Convert.ToInt32(byteArray.Substring(16, 2), 16) & 0x3f) | 0x80;

            byteArray =
                byteArray[..12] +
                byte6.ToString("x2") +
                byteArray.Substring(14, 2) +
                byte8.ToString("x2") +
                byteArray[18..];

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