using System;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciAPI.Client;
using NuciDAL.Repositories;

using NuciLog;
using NuciLog.Core;

using NuciText.Normalisation;
using NuciText.Obfuscation;

using NuciWeb.HTTP;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.DataAccess.Repositories;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Anonymisation;
using NuciCraft.API.Service.Generators;

namespace NuciCraft.API
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            DataStoreSettings dataStoreSettings = new();
            SecuritySettings securitySettings = new();
            ServerSettings serverSettings = new();
            RtpLocationSettings rtpLocationSettings = new();
            UniversalNameGeneratorSettings universalNameGeneratorSettings = new();
            WebMapSettings webMapSettings = new();
            AccessControlSettings accessControlSettings = new();

            configuration.Bind(nameof(dataStoreSettings), dataStoreSettings);
            configuration.Bind(nameof(securitySettings), securitySettings);
            configuration.Bind(nameof(serverSettings), serverSettings);
            configuration.Bind(nameof(rtpLocationSettings), rtpLocationSettings);
            configuration.Bind(nameof(webMapSettings), webMapSettings);
            configuration.Bind(
                nameof(universalNameGeneratorSettings),
                universalNameGeneratorSettings);
            configuration.Bind(nameof(accessControlSettings), accessControlSettings);

            return services
                .AddSingleton(dataStoreSettings)
                .AddSingleton(securitySettings)
                .AddSingleton(serverSettings)
                .AddSingleton(rtpLocationSettings)
                .AddSingleton(universalNameGeneratorSettings)
                .AddSingleton(webMapSettings)
                .AddSingleton(accessControlSettings)
                .AddNuciLoggerSettings(configuration);
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services) => services
            .AddMemoryCache()
            .AddSingleton(TimeProvider.System)
            .AddSingleton(_ => HttpClientCreator.Create())
            .AddJsonRepository<PlayerDataObject>(settings => settings.PlayersStorePath)
            .AddJsonRepository<RtpLocationEntity>(settings => settings.RtpLocationsStorePath)
            .AddJsonRepository<CountryDataObject>(settings => settings.CountriesStorePath)
            .AddJsonRepository<HomeDataObject>(settings => settings.HomesStorePath)
            .AddJsonRepository<ItemDataObject>(settings => settings.ItemsStorePath)
            .AddJsonRepository<WorldDataObject>(settings => settings.WorldsStorePath)
            .AddJsonRepository<ZoneDataObject>(settings => settings.ZonesStorePath)
            .AddJsonRepository<ZoneTypeDataObject>(settings => settings.ZoneTypesStorePath)
            .AddSingleton<INuciApiClient>(serviceProvider =>
                new NuciApiClient(
                    serviceProvider
                        .GetRequiredService<UniversalNameGeneratorSettings>()
                        .BaseUrl))
            .AddSingleton<IMobService, MobService>()
            .AddSingleton<IServerStatusService, ServerStatusService>()
            .AddSingleton<ICountryService, CountryService>()
            .AddSingleton<IHomeService, HomeService>()
            .AddSingleton<IItemService, ItemService>()
            .AddSingleton<IWorldService, WorldService>()
            .AddSingleton<IZoneTypeService, ZoneTypeService>()
            .AddSingleton<IPlayerService, PlayerService>()
            .AddSingleton<IRtpLocationService, RtpLocationService>()
            .AddSingleton<IZoneService, ZoneService>()
            .AddSingleton<IAccessControlService, AccessControlService>()
            .AddSingleton<IAnonymisationServiceDetectorFactory, AnonymisationServiceDetectorFactory>()
            .AddSingleton<ISignIdGenerator, SignIdGenerator>()
            .AddSingleton<INuciTextNormaliser, NuciTextNormaliser>()
            .AddSingleton<INuciTextObfuscator, NuciTextObfuscator>()
            .AddScoped<ILogger, NuciLogger>();

        private static IServiceCollection AddJsonRepository<TDataObject>(
            this IServiceCollection services,
            Func<DataStoreSettings, string> storePathProvider)
            where TDataObject : NuciCraftEntityBase
            => services.AddSingleton<IFileRepository<TDataObject>>(
                serviceProvider => new OptimisticJsonRepository<TDataObject>(
                    storePathProvider(
                        serviceProvider.GetRequiredService<DataStoreSettings>())));
    }
}
