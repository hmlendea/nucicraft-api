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

        private static string RomanianMaleFullNamesSchema => "romanian-persons-male";

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
                MobType mobType = GetMobType(request.MobType);
                GenerateNamesRequest generateNamesRequest = BuildGenerateNamesRequest(
                    mobType);
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
                    mobType);

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

        private GenerateNamesRequest BuildGenerateNamesRequest(MobType mobType) => new()
        {
            ApiKey = settings.ApiKey,
            Schema = GetSchemaForMobType(mobType),
            Count = GeneratedNameCount
        };

        private static string ExtractGeneratedName(
            GenerateNamesResponse generateNamesResponse,
            MobType mobType)
        {
            if (generateNamesResponse is null)
            {
                throw new InvalidOperationException(
                    "The Universal Name Generator API response could not be deserialised.");
            }

            string generatedName = null;

            if (generateNamesResponse.Names is not null)
            {
                generatedName = generateNamesResponse.Names.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(generatedName))
            {
                throw new InvalidOperationException(
                    $"The Universal Name Generator API returned no names for the '{mobType}' mob type.");
            }

            return generatedName;
        }

        private static MobType GetMobType(string mobTypeName)
        {
            MobType mobType = MobType.FromString(mobTypeName);

            if (object.Equals(mobType, MobType.Unsupported))
            {
                throw new NotImplementedException(
                    $"The '{mobTypeName}' mob type is not supported.");
            }

            return mobType;
        }

        private static string GetSchemaForMobType(MobType mobType)
        {
            if (object.Equals(mobType, MobType.WanderingTrader))
            {
                return RomanianMaleFullNamesSchema;
            }

            throw new NotImplementedException(
                $"The '{mobType}' mob type does not have a configured schema.");
        }

        private void ValidateSettings()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.BaseUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);
        }
    }
}