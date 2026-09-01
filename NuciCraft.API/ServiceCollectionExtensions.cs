using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciAPI.Client;

using NuciDAL.DataObjects;
using NuciDAL.Repositories;

using NuciLog;
using NuciLog.Core;

using NuciText.Normalisation;
using NuciText.Obfuscation;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service;

namespace NuciCraft.API
{
    public static class ServiceCollectionExtensions
    {
        private static DataStoreSettings dataStoreSettings;

        public static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            dataStoreSettings = new DataStoreSettings();
            SecuritySettings securitySettings = new();
            RtpLocationSettings rtpLocationSettings = new();
            UniversalNameGeneratorSettings universalNameGeneratorSettings = new();

            configuration.Bind(nameof(dataStoreSettings), dataStoreSettings);
            configuration.Bind(nameof(securitySettings), securitySettings);
            configuration.Bind(nameof(rtpLocationSettings), rtpLocationSettings);
            configuration.Bind(
                nameof(universalNameGeneratorSettings),
                universalNameGeneratorSettings);

            return services
                .AddSingleton(dataStoreSettings)
                .AddSingleton(securitySettings)
                .AddSingleton(rtpLocationSettings)
                .AddSingleton(universalNameGeneratorSettings)
                .AddNuciLoggerSettings(configuration);
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services) => services
            .AddJsonRepository<PlayerDataObject>(() => dataStoreSettings.PlayersStorePath)
            .AddJsonRepository<RtpLocationEntity>(() => dataStoreSettings.RtpLocationsStorePath)
            .AddJsonRepository<CountryDataObject>(() => dataStoreSettings.CountriesStorePath)
            .AddJsonRepository<WorldDataObject>(() => dataStoreSettings.WorldsStorePath)
            .AddJsonRepository<ZoneDataObject>(() => dataStoreSettings.ZonesStorePath)
            .AddJsonRepository<ZoneTypeDataObject>(() => dataStoreSettings.ZoneTypesStorePath)
            .AddSingleton<INuciApiClient>(serviceProvider =>
                new NuciApiClient(
                    serviceProvider
                        .GetRequiredService<UniversalNameGeneratorSettings>()
                        .BaseUrl))
            .AddSingleton<IMobService, MobService>()
            .AddSingleton<ICountryService, CountryService>()
            .AddSingleton<IWorldService, WorldService>()
            .AddSingleton<IZoneTypeService, ZoneTypeService>()
            .AddSingleton<IPlayerService, PlayerService>()
            .AddSingleton<IRtpLocationService, RtpLocationService>()
            .AddSingleton<IZoneService, ZoneService>()
            .AddSingleton<INuciTextNormaliser, NuciTextNormaliser>()
            .AddSingleton<INuciTextObfuscator, NuciTextObfuscator>()
            .AddScoped<ILogger, NuciLogger>();

        private static IServiceCollection AddJsonRepository<TDataObject>(
            this IServiceCollection services,
            Func<string> storePathProvider)
            where TDataObject : EntityBase
            => services.AddSingleton<IFileRepository<TDataObject>>(
                _ => new JsonRepository<TDataObject>(storePathProvider()));
    }
}
