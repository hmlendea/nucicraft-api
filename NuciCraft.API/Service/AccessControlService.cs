using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NuciLog.Core;
using NuciWeb.HTTP;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class AccessControlService : IAccessControlService
    {
        private static string UsernameNotPermittedMessage => "This username is not permitted.";

        private readonly HttpClient httpClient;
        private readonly AccessControlSettings settings;
        private readonly ILogger logger;

        public AccessControlService(
            AccessControlSettings settings,
            ILogger logger)
            : this(settings, logger, HttpClientCreator.Create())
        {
        }

        public AccessControlService(
            AccessControlSettings settings,
            ILogger logger,
            HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
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
                new(MyLogInfoKey.IpAddress, request.IpAddress)
            ];

            logger.Info(
                MyOperation.CheckIp,
                OperationStatus.Started,
                logInfos);

            try
            {
                IpCheckResult response = await PerformIpCheck(request.IpAddress, cancellationToken);

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

        private async Task<IpCheckResult> PerformIpCheck(string ipAddress, CancellationToken cancellationToken)
        {
            GeolocationData? geo = await GetGeolocationData(ipAddress, cancellationToken);

            if (geo is null || !string.Equals(geo.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return new IpCheckResult
                {
                    IsAllowed = true,
                    CountryCode = null,
                    DeniedReason = null
                };
            }

            string countryCode = geo.CountryCode;

            if (Array.Exists(settings.DeniedCountries, c => string.Equals(c, countryCode, StringComparison.OrdinalIgnoreCase)))
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = countryCode,
                    DeniedReason = "Access is not permitted from this country."
                };
            }

            if (await IsAnonymisationServiceDetected(ipAddress, cancellationToken))
            {
                return new IpCheckResult
                {
                    IsAllowed = false,
                    CountryCode = countryCode,
                    DeniedReason = "Access is not permitted via any anonymisation service."
                };
            }

            return new IpCheckResult
            {
                IsAllowed = true,
                CountryCode = countryCode,
                DeniedReason = null
            };
        }

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
                string url = $"{detectionSettings.ApiUrl}?ip={Uri.EscapeDataString(ipAddress)}&license={Uri.EscapeDataString(detectionSettings.ApiLicense)}&cache={detectionSettings.IsCacheEnabled.ToString().ToLowerInvariant()}";

                using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                AnonymisationServiceCheckData? anonymisationServiceCheck = JsonSerializer.Deserialize<AnonymisationServiceCheckData>(json);

                return anonymisationServiceCheck is not null && anonymisationServiceCheck.Success && anonymisationServiceCheck.IsAnonymisationService;
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