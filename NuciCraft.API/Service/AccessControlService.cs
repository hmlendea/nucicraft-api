using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciLog.Core;
using NuciWeb.HTTP;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public sealed class AccessControlService(
        AccessControlSettings settings,
        ILogger logger) : IAccessControlService
    {
        private readonly HttpClient httpClient = HttpClientCreator.Create();

        public IpCheckResult CheckIp(CheckIpRequest request)
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
                IpCheckResult response = PerformIpCheck(request.IpAddress).GetAwaiter().GetResult();

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

        public UsernameCheckResult CheckUsername(CheckUsernameRequest request)
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
                UsernameCheckResult response = PerformUsernameCheck(request.Username);

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

        private async Task<IpCheckResult> PerformIpCheck(string ipAddress)
        {
            string url = $"{settings.GeolocationApiUrl}{ipAddress}?fields=status,countryCode,country";

            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            GeolocationData? geo = JsonSerializer.Deserialize<GeolocationData>(json);

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
                    DeniedReason = "Access denied from your country"
                };
            }

            return new IpCheckResult
            {
                IsAllowed = true,
                CountryCode = countryCode,
                DeniedReason = null
            };
        }

        private UsernameCheckResult PerformUsernameCheck(string username)
        {
            if (username.Length < settings.MinimumUsernameLength)
            {
                return new UsernameCheckResult
                {
                    IsAllowed = false,
                    DeniedReason = "Unallowed username. Use another one"
                };
            }

            foreach (string pattern in settings.DeniedUsernamePatterns)
            {
                if (Regex.IsMatch(username, pattern))
                {
                    return new UsernameCheckResult
                    {
                        IsAllowed = false,
                        DeniedReason = "Unallowed username. Use another one"
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