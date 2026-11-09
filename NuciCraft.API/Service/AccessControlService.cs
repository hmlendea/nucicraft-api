using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NuciDAL.Repositories;

using NuciLog.Core;
using NuciWeb.HTTP;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Anonymisation;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class AccessControlService : IAccessControlService
    {
        private static string UsernameNotPermittedMessage => "This username is not permitted.";

        private static string DeniedCountryMessage => "Access is not permitted from this country.";

        private static string AnonymisationServiceMessage => "Access is not permitted via any anonymisation service.";

        private static string SuccessfulGeolocationStatus => "success";

        private static string BuildBannedIpAddressMessage(string serverName) => $"This IP address is banned on {serverName}";

        private static string BuildBannedIpAddressOfUserMessage(string serverName) => $"This IP address is associated with a user banned on {serverName}";

        private static string BuildBannedUsernameMessage(string banReason)
        {
            if (string.IsNullOrWhiteSpace(banReason))
            {
                return "This user is banned.";
            }

            return $"This user is banned. {banReason}";
        }

        private readonly HttpClient httpClient;
        private readonly AccessControlSettings settings;
        private readonly ServerSettings serverSettings;
        private readonly IAnonymisationServiceDetectorFactory anonymisationServiceDetectorFactory;
        private readonly IFileRepository<PlayerDataObject> playerRepository;
        private readonly ILogger logger;

        public AccessControlService(
            AccessControlSettings settings,
            ServerSettings serverSettings,
            IAnonymisationServiceDetectorFactory anonymisationServiceDetectorFactory,
            IFileRepository<PlayerDataObject> playerRepository,
            ILogger logger)
            : this(
                settings,
                serverSettings,
                anonymisationServiceDetectorFactory,
                playerRepository,
                logger,
                HttpClientCreator.Create())
        {
        }

        public AccessControlService(
            AccessControlSettings settings,
            ServerSettings serverSettings,
            IAnonymisationServiceDetectorFactory anonymisationServiceDetectorFactory,
            IFileRepository<PlayerDataObject> playerRepository,
            ILogger logger,
            HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(serverSettings);
            ArgumentNullException.ThrowIfNull(anonymisationServiceDetectorFactory);
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.serverSettings = serverSettings;
            this.anonymisationServiceDetectorFactory = anonymisationServiceDetectorFactory;
            this.playerRepository = playerRepository;
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public Task<IpCheckResult> CheckIpAsync(CheckIpRequest request)
            => CheckIpAsync(request, CancellationToken.None);

        public async Task<IpCheckResult> CheckIpAsync(CheckIpRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.IpAddress, request.IpAddress),
                new(MyLogInfoKey.Username, request.Username)
            ];

            logger.Info(
                MyOperation.CheckIp,
                OperationStatus.Started,
                logInfos);

            try
            {
                IpCheckResult response = await PerformIpCheck(request, cancellationToken);

                logger.Info(
                    MyOperation.CheckIp,
                    OperationStatus.Success,
                    logInfos);

                return response;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.CheckIp,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        public Task<UsernameCheckResult> CheckUsernameAsync(CheckUsernameRequest request)
            => CheckUsernameAsync(request, CancellationToken.None);

        public async Task<UsernameCheckResult> CheckUsernameAsync(CheckUsernameRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Username)
            ];

            logger.Info(
                MyOperation.CheckUsername,
                OperationStatus.Started,
                logInfos);

            try
            {
                UsernameCheckResult response = await PerformUsernameCheckAsync(request.Username, cancellationToken);

                logger.Info(
                    MyOperation.CheckUsername,
                    OperationStatus.Success,
                    logInfos);

                return response;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.CheckUsername,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private async Task<IpCheckResult> PerformIpCheck(CheckIpRequest request, CancellationToken cancellationToken)
        {
            PlayerDataObject[] players = [.. playerRepository.GetAll()];
            PlayerDataObject? requestedPlayer = FindPlayerByUsername(request.Username, players);

            if (settings.IsRegisteredPlayerExemptionEnabled && requestedPlayer is not null && !requestedPlayer.IsBanned)
            {
                return new IpCheckResult
                {
                    IsAllowed = true,
                    CountryCode = null,
                    DeniedReason = null
                };
            }

            if (requestedPlayer is not null && requestedPlayer.IsBanned)
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = null,
                    DeniedReason = BuildBannedIpAddressOfUserMessage(serverSettings.Name)
                };
            }

            if (IsIpAddressAssociatedWithBannedPlayer(request.IpAddress, players))
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = null,
                    DeniedReason = BuildBannedIpAddressDenialMessage(request.Username)
                };
            }

            return await PerformRegularIpCheck(request.IpAddress, cancellationToken);
        }

        private string BuildBannedIpAddressDenialMessage(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BuildBannedIpAddressMessage(serverSettings.Name);
            }

            return BuildBannedIpAddressOfUserMessage(serverSettings.Name);
        }

        private async Task<IpCheckResult> PerformRegularIpCheck(
            string ipAddress,
            CancellationToken cancellationToken)
        {
            GeolocationData? geolocationData = await GetGeolocationData(ipAddress, cancellationToken);

            if (geolocationData is null || !IsSuccessfulGeolocationStatus(geolocationData.Status))
            {
                return new IpCheckResult
                {
                    IsAllowed = true,
                    CountryCode = null,
                    DeniedReason = null
                };
            }

            string countryCode = geolocationData.CountryCode;

            if (IsDeniedCountry(countryCode))
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = countryCode,
                    DeniedReason = DeniedCountryMessage
                };
            }

            return await PerformAnonymisationServiceCheck(ipAddress, countryCode, cancellationToken);
        }

        private async Task<IpCheckResult> PerformAnonymisationServiceCheck(
            string ipAddress,
            string countryCode,
            CancellationToken cancellationToken)
        {
            if (await IsAnonymisationServiceDetected(ipAddress, cancellationToken))
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = countryCode,
                    DeniedReason = AnonymisationServiceMessage
                };
            }

            return new IpCheckResult
            {
                IsAllowed = true,
                CountryCode = countryCode,
                DeniedReason = null
            };
        }

        private static bool IsSuccessfulGeolocationStatus(string status)
            => string.Equals(status, SuccessfulGeolocationStatus, StringComparison.OrdinalIgnoreCase);

        private bool IsDeniedCountry(string countryCode)
            => Array.Exists(
                settings.DeniedCountries,
                deniedCountry => string.Equals(
                    deniedCountry,
                    countryCode,
                    StringComparison.OrdinalIgnoreCase));

        private static PlayerDataObject? FindPlayerByUsername(
            string? username,
            IEnumerable<PlayerDataObject> players)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return players.FirstOrDefault(player =>
                string.Equals(player.Username, username, StringComparison.Ordinal));
        }

        private static bool IsIpAddressAssociatedWithBannedPlayer(
            string ipAddress,
            IEnumerable<PlayerDataObject> players)
            => players.Any(player =>
                player.IsBanned &&
                string.Equals(player.LastIpAddress, ipAddress, StringComparison.Ordinal));

        private async Task<GeolocationData?> GetGeolocationData(string ipAddress, CancellationToken cancellationToken)
        {
            string url = $"{settings.GeolocationApiUrl}{ipAddress}?fields=status,countryCode,country";

            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonSerializer.Deserialize<GeolocationData>(json);
        }

        private async Task<bool> IsAnonymisationServiceDetected(string ipAddress, CancellationToken cancellationToken)
        {
            AnonymisationServiceDetectionSettings detectionSettings = settings.AnonymisationServiceDetection;

            if (!detectionSettings.IsEnabled)
            {
                return false;
            }

            try
            {
                IAnonymisationServiceDetector detector = anonymisationServiceDetectorFactory.Create();

                return await detector.IsAnonymisationService(ipAddress, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.CheckIp,
                    OperationStatus.Failure,
                    exception,
                    [new(MyLogInfoKey.IpAddress, ipAddress)]);

                return false;
            }
        }

        private async Task<UsernameCheckResult> PerformUsernameCheckAsync(string username, CancellationToken cancellationToken)
        {
            PlayerDataObject[] players = [.. playerRepository.GetAll()];
            PlayerDataObject? requestedPlayer = FindPlayerByUsername(username, players);

            if (requestedPlayer is not null && requestedPlayer.IsBanned)
            {
                return new UsernameCheckResult
                {
                    IsAllowed = false,
                    DeniedReason = BuildBannedUsernameMessage(requestedPlayer.BannedReason)
                };
            }

            if (settings.IsRegisteredPlayerExemptionEnabled && requestedPlayer is not null)
            {
                return new UsernameCheckResult
                {
                    IsAllowed = true,
                    DeniedReason = null
                };
            }

            if (username.Length < settings.MinimumUsernameLength)
            {
                return new UsernameCheckResult
                {
                    IsAllowed = false,
                    DeniedReason = UsernameNotPermittedMessage
                };
            }

            foreach (string pattern in settings.DeniedUsernamePatterns)
            {
                if (Regex.IsMatch(username, pattern))
                {
                    return new UsernameCheckResult
                    {
                        IsAllowed = false,
                        DeniedReason = UsernameNotPermittedMessage
                    };
                }
            }

            return new UsernameCheckResult
            {
                IsAllowed = true,
                DeniedReason = null
            };
        }
    }
}
