using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using NuciAPI.Client;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;
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

        private static string RomanianFemaleFullNamesSchema => "romanian-persons-female";

        private static string DragonNamesSchema => "fantasy-dragons";

        private static string CowNamesSchema => "romanian-animals-cows";

        private static string PigNamesSchema => "romanian-animals-pigs";

        private static string ZaganianMaleNamesSchema => "pinched-zaganian-persons-male";

        private static int VillageSchemaVariantsCount => 2;

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
                NuciApiRequestAuthorisationInfo requestAuthorisationInfo =
                    BuildRequestAuthorisationInfo();
                NuciApiResponse apiResponse = universalNameGeneratorClient
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        generateNamesRequest,
                        requestAuthorisationInfo,
                        NamesEndpoint)
                    .GetAwaiter()
                    .GetResult();
                string generatedName = ExtractGeneratedName(
                    apiResponse,
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
            Schema = GetSchemaForMobType(mobType),
            Count = GeneratedNameCount
        };

        private NuciApiRequestAuthorisationInfo BuildRequestAuthorisationInfo() => new()
        {
            BearerToken = settings.ApiKey
        };

        private static string ExtractGeneratedName(
            NuciApiResponse apiResponse,
            MobType mobType)
        {
            ArgumentNullException.ThrowIfNull(apiResponse);

            if (!apiResponse.IsSuccessful)
            {
                throw new InvalidOperationException(
                    $"The Universal Name Generator API request has failed with the '{apiResponse.Code}' code: {apiResponse.Message}");
            }

            GenerateNamesResponse generateNamesResponse =
                apiResponse as GenerateNamesResponse ??
                throw new InvalidOperationException(
                    $"The Universal Name Generator API returned an unexpected response type: '{apiResponse.GetType().Name}'.");
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

            if (Equals(mobType, MobType.Unsupported))
            {
                throw new NotImplementedException(
                    $"The '{mobTypeName}' mob type is not supported.");
            }

            return mobType;
        }

        private static string GetSchemaForMobType(MobType mobType)
        {
            if (Equals(mobType, MobType.WanderingTrader))
            {
                return RomanianMaleFullNamesSchema;
            }

            if (Equals(mobType, MobType.EnderDragon))
            {
                return DragonNamesSchema;
            }

            if (Equals(mobType, MobType.Cow))
            {
                return CowNamesSchema;
            }

            if (Equals(mobType, MobType.Pig))
            {
                return PigNamesSchema;
            }

            if (UsesZaganianMaleNamesSchema(mobType))
            {
                return ZaganianMaleNamesSchema;
            }

            if (Equals(mobType, MobType.Villager))
            {
                return GetRandomVillageSchema();
            }

            throw new NotImplementedException(
                $"The '{mobType}' mob type does not have a configured schema.");
        }

        private static bool UsesZaganianMaleNamesSchema(MobType mobType)
        {
            if (Equals(mobType, MobType.Evoker))
            {
                return true;
            }

            if (Equals(mobType, MobType.Illusioner))
            {
                return true;
            }

            if (Equals(mobType, MobType.Pillager))
            {
                return true;
            }

            if (Equals(mobType, MobType.Vindicator))
            {
                return true;
            }

            return false;
        }

        private static string GetRandomVillageSchema()
        {
            int villageSchemaVariant = Random.Shared.Next(VillageSchemaVariantsCount);

            if (villageSchemaVariant == 0)
            {
                return RomanianMaleFullNamesSchema;
            }

            return RomanianFemaleFullNamesSchema;
        }

        private void ValidateSettings()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.BaseUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);
        }
    }
}