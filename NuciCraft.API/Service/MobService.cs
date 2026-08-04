using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using NuciAPI.Client;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;

using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public sealed class MobService(
        INuciApiClient universalNameGeneratorClient,
        UniversalNameGeneratorSettings settings,
        ILogger logger) : IMobService
    {
        private static int GeneratedNameCount => 1;

        private static string NamesEndpoint => "Names";

        private static string WanderingTraderMobType => "wandering_trader";

        private static string WanderingTraderSchema => "romanian-persons-male";

        public string GetRandomMobName(GetMobNameRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.MobType);

            ValidateSettings();

            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.MobType, request.MobType),
                new(MyLogInfoKey.Count, GeneratedNameCount)
            ];

            logger.Info(
                MyOperation.GetRandomMobName,
                OperationStatus.Started,
                logInfos);

            try
            {
                GenerateNamesRequest generateNamesRequest = BuildGenerateNamesRequest(
                    request.MobType);
                NuciApiResponse apiResponse = universalNameGeneratorClient
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        generateNamesRequest,
                        NamesEndpoint)
                    .GetAwaiter()
                    .GetResult();
                GenerateNamesResponse generateNamesResponse =
                    apiResponse as GenerateNamesResponse;
                string generatedName = ExtractGeneratedName(
                    generateNamesResponse,
                    request.MobType);

                logger.Info(
                    MyOperation.GetRandomMobName,
                    OperationStatus.Success,
                    logInfos);

                return generatedName;
            }
            catch (Exception exception)
            {
                logger.Error(
                    MyOperation.GetRandomMobName,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private GenerateNamesRequest BuildGenerateNamesRequest(string mobType) => new()
        {
            ApiKey = settings.ApiKey,
            Schema = GetSchemaForMobType(mobType),
            Count = GeneratedNameCount
        };

        private static string ExtractGeneratedName(
            GenerateNamesResponse generateNamesResponse,
            string mobType)
        {
            if (generateNamesResponse is null)
            {
                throw new InvalidOperationException(
                    "The Universal Name Generator API response could not be deserialised.");
            }

            string generatedName = generateNamesResponse.Names.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(generatedName))
            {
                throw new InvalidOperationException(
                    $"The Universal Name Generator API returned no names for the '{mobType}' mob type.");
            }

            return generatedName;
        }

        private static string GetSchemaForMobType(string mobType)
        {
            if (string.Equals(
                mobType,
                WanderingTraderMobType,
                StringComparison.OrdinalIgnoreCase))
            {
                return WanderingTraderSchema;
            }

            throw new NotImplementedException(
                $"The '{mobType}' mob type is not supported.");
        }

        private void ValidateSettings()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.BaseUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);
        }
    }
}