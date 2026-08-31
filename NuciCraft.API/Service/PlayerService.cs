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
        private static string OfflinePlayerNamePrefix => "OfflinePlayer:";

        private static int OfflineUuidVersionByteIndex => 6;

        private static int OfflineUuidVariantByteIndex => 8;

        private static byte UuidVersionMask => 0x0f;

        private static byte UuidVersionThreeBits => 0x30;

        private static byte UuidVariantMask => 0x3f;

        private static byte UuidRfc4122VariantBits => 0x80;

        public void Register(RegisterPlayerRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Username),
                new(MyLogInfoKey.OnlineUUID, request.OnlineUUID),
                new(MyLogInfoKey.CreatedDT, request.CreatedDT),
                new(MyLogInfoKey.LastIpAddress, request.LastIpAddress)
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
                    DisplayName = request.DisplayName,
                    Gender = Gender.FromString(request.Gender),
                    OfflineUUID = GetOfflineUuid(request.Username),
                    OnlineUUID = request.OnlineUUID,
                    Password = request.Password,
                    CreatedDT = GetCreatedDateTimeForRegisterRequest(request),
                    LastIpAddress = request.LastIpAddress,
                    WikiUrl = request.WikiUrl,
                    IsBanned = request.IsBanned,
                    BannedDT = ParseOptionalTimestamp(request.BannedDT, nameof(request.BannedDT)),
                    IsMuted = request.IsMuted,
                    MutedDT = ParseOptionalTimestamp(request.MutedDT, nameof(request.MutedDT)),
                    LastLoginDT = ParseOptionalTimestamp(request.LastLoginDT, nameof(request.LastLoginDT)),
                    LastLogoutDT = ParseOptionalTimestamp(request.LastLogoutDT, nameof(request.LastLogoutDT)),
                    LastLogoutLocation = request.LastLogoutLocation?.ToServiceModel(),
                    LastSleptLocation = request.LastSleptLocation?.ToServiceModel(),
                    BedLocation = request.BedLocation?.ToServiceModel(),
                    BackDT = ParseOptionalTimestamp(request.BackDT, nameof(request.BackDT)),
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
                PlayerDataObject matchingDataObject = FindPlayerDataObject(
                    request.Identifier,
                    request.Username,
                    request.OfflineUUID,
                    request.OnlineUUID);

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

                PlayerDataObject playerDataObject = FindPlayerDataObject(
                    request.Identifier,
                    request.Username,
                    request.OfflineUUID,
                    request.OnlineUUID);

                ApplyPatchValues(request, playerDataObject);

                playerDataObject.UpdatedDT = TimestampFormats.GetCurrentUtcTimestamp();

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
            string playerOnlineUuid) =>
            playerDataObject =>
                MatchesPlayerSelector(playerIdentifier, playerDataObject.Id) ||
                MatchesPlayerSelector(playerUsername, playerDataObject.Username) ||
                MatchesPlayerSelector(playerOfflineUuid, playerDataObject.OfflineUUID) ||
                MatchesPlayerSelector(playerOnlineUuid, playerDataObject.OnlineUUID);

        private PlayerDataObject FindPlayerDataObject(
            string playerIdentifier,
            string playerUsername,
            string playerOfflineUuid,
            string playerOnlineUuid)
        {
            Func<PlayerDataObject, bool> matchesRequest = BuildPlayerDataObjectMatcher(
                playerIdentifier,
                playerUsername,
                playerOfflineUuid,
                playerOnlineUuid);

            PlayerDataObject matchingDataObject = repository
                .GetAll()
                .FirstOrDefault(matchesRequest);

            if (matchingDataObject is null)
            {
                throw new KeyNotFoundException("No player found matching the provided criteria.");
            }

            return matchingDataObject;
        }

        private static bool MatchesPlayerSelector(string selectorValue, string playerValue)
            => !string.IsNullOrWhiteSpace(selectorValue) &&
                string.Equals(playerValue, selectorValue);

        private static void ApplyPatchValues(
            PatchPlayerRequest request,
            PlayerDataObject playerDataObject)
        {
            if (request.DisplayName is not null)
            {
                playerDataObject.DisplayName = request.DisplayName;
            }

            if (request.Password is not null)
            {
                playerDataObject.Password = request.Password;
            }

            if (request.LastIpAddress is not null)
            {
                playerDataObject.LastIpAddress = request.LastIpAddress;
            }

            if (request.DiscordId is not null)
            {
                playerDataObject.DiscordId = request.DiscordId;
            }

            if (request.EmailAddress is not null)
            {
                playerDataObject.EmailAddress = request.EmailAddress;
            }

            if (request.WikiUrl is not null)
            {
                playerDataObject.WikiUrl = request.WikiUrl;
            }

            playerDataObject.IsBanned = request.IsBanned;

            if (request.BannedDT is not null)
            {
                playerDataObject.BannedDT = request.BannedDT;
            }

            playerDataObject.IsMuted = request.IsMuted;

            if (request.MutedDT is not null)
            {
                playerDataObject.MutedDT = request.MutedDT;
            }

            if (request.LastLoginDT is not null)
            {
                playerDataObject.LastLoginDT = request.LastLoginDT;
            }

            if (request.LastLogoutDT is not null)
            {
                playerDataObject.LastLogoutDT = request.LastLogoutDT;
            }

            if (request.LastLogoutLocation is not null)
            {
                playerDataObject.LastLogoutLocation = request.LastLogoutLocation;
            }

            if (request.LastSleptDT is not null)
            {
                playerDataObject.LastSleptDT = request.LastSleptDT;
            }

            if (request.LastSleptLocation is not null)
            {
                playerDataObject.LastSleptLocation = request.LastSleptLocation;
            }

            if (request.BedLocation is not null)
            {
                playerDataObject.BedLocation = request.BedLocation;
            }

            if (request.Gender is not null)
            {
                playerDataObject.Gender = Gender.FromString(request.Gender);
            }

            if (request.LastDeathDT is not null)
            {
                playerDataObject.LastDeathDT = request.LastDeathDT;
            }

            if (request.LastDeathLocation is not null)
            {
                playerDataObject.LastDeathLocation = request.LastDeathLocation;
            }

            if (request.BackDT is not null)
            {
                playerDataObject.BackDT = request.BackDT;
            }

            if (request.BackLocation is not null)
            {
                playerDataObject.BackLocation = request.BackLocation;
            }

            if (request.Settings is not null)
            {
                playerDataObject.Settings = playerDataObject.Settings.MergeWith(request.Settings);
            }
        }

        private static DateTimeOffset GetCreatedDateTimeForRegisterRequest(RegisterPlayerRequest request)
        {
            if (request.CreatedDT is null)
            {
                return DateTimeOffset.UtcNow;
            }

            return ParseTimestamp(request.CreatedDT, nameof(request.CreatedDT));
        }

        private static DateTimeOffset? ParseOptionalTimestamp(
            string timestamp,
            string timestampName)
        {
            if (timestamp is null)
            {
                return null;
            }

            return ParseTimestamp(timestamp, timestampName);
        }

        private static DateTimeOffset ParseTimestamp(
            string timestamp,
            string timestampName)
        {
            if (DateTimeOffset.TryParseExact(
                    timestamp,
                    TimestampFormats.Full,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTimeOffset dateTimeOffset))
            {
                return dateTimeOffset;
            }

            throw new ArgumentException(
                $"The {timestampName} timestamp must match format '{TimestampFormats.Full}'.",
                timestampName);
        }

        private static string GetOfflineUuid(string username)
        {
            byte[] hashBytes = MD5.HashData(
                Encoding.UTF8.GetBytes($"{OfflinePlayerNamePrefix}{username}"));

            hashBytes[OfflineUuidVersionByteIndex] = (byte)(
                hashBytes[OfflineUuidVersionByteIndex] & UuidVersionMask |
                UuidVersionThreeBits);
            hashBytes[OfflineUuidVariantByteIndex] = (byte)(
                hashBytes[OfflineUuidVariantByteIndex] & UuidVariantMask |
                UuidRfc4122VariantBits);

            Guid offlineUuid = new(hashBytes, bigEndian: true);

            return offlineUuid.ToString();
        }
    }
}
