using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciAPI.Client;

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
            .AddSingleton<IFileRepository<PlayerDataObject>>(serviceProvider =>
                new JsonRepository<PlayerDataObject>(
                    dataStoreSettings.PlayersStorePath))
            .AddSingleton<IFileRepository<RtpLocationEntity>>(serviceProvider =>
                new JsonRepository<RtpLocationEntity>(
                    dataStoreSettings.RtpLocationsStorePath))
            .AddSingleton<IFileRepository<CountryDataObject>>(serviceProvider =>
                new JsonRepository<CountryDataObject>(
                    dataStoreSettings.CountriesStorePath))
            .AddSingleton<IFileRepository<ZoneDataObject>>(serviceProvider =>
                new JsonRepository<ZoneDataObject>(
                    dataStoreSettings.ZonesStorePath))
            .AddSingleton<INuciApiClient>(serviceProvider =>
                new NuciApiClient(
                    serviceProvider
                        .GetRequiredService<UniversalNameGeneratorSettings>()
                        .BaseUrl))
            .AddSingleton<IMobService, MobService>()
            .AddSingleton<ICountryService, CountryService>()
            .AddSingleton<IPlayerService, PlayerService>()
            .AddSingleton<IRtpLocationService, RtpLocationService>()
            .AddSingleton<IZoneService, ZoneService>()
            .AddSingleton<INuciTextNormaliser, NuciTextNormaliser>()
            .AddSingleton<INuciTextObfuscator, NuciTextObfuscator>()
            .AddScoped<ILogger, NuciLogger>();
    }
}
