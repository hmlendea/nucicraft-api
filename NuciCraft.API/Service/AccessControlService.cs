using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NuciLog.Core;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;
using NuciCraft.API.Service;

namespace NuciCraft.API.Service
{
    public sealed class AccessControlService(
        AccessControlSettings settings,
        ILogger logger) : IAccessControlService
    {
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
                IpCheckResult response = PerformIpCheck(request.IpAddress);

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

        private IpCheckResult PerformIpCheck(string ipAddress) => new()
        {
            IsAllowed = true,
            CountryCode = null,
            DeniedReason = null
        };

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
                if (username.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return new UsernameCheckResult
                    {
                        IsAllowed = false,
                        DeniedReason = "Unallowed username. Use another one"
                    };
                }
            }

            foreach (string exactName in settings.DeniedExactUsernames)
            {
                if (string.Equals(username, exactName, StringComparison.OrdinalIgnoreCase))
                {
                    return new UsernameCheckResult
                    {
                        IsAllowed = false,
                        DeniedReason = "Unknown host"
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