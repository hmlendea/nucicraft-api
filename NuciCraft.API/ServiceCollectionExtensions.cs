using System;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            services
                .Configure<DataStoreSettings>(configuration.GetSection(nameof(DataStoreSettings)))
                .Configure<SecuritySettings>(configuration.GetSection(nameof(SecuritySettings)))
                .Configure<ServerSettings>(configuration.GetSection(nameof(ServerSettings)))
                .Configure<RtpLocationSettings>(configuration.GetSection(nameof(RtpLocationSettings)))
                .Configure<UniversalNameGeneratorSettings>(configuration.GetSection(nameof(UniversalNameGeneratorSettings)))
                .Configure<WebMapSettings>(configuration.GetSection(nameof(WebMapSettings)))
                .Configure<AccessControlSettings>(configuration.GetSection(nameof(AccessControlSettings)))
                .AddNuciLoggerSettings(configuration);

            services
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<DataStoreSettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<SecuritySettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<ServerSettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<RtpLocationSettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<UniversalNameGeneratorSettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<WebMapSettings>>().CurrentValue)
                .AddSingleton(sp => sp.GetRequiredService<IOptionsMonitor<AccessControlSettings>>().CurrentValue);

            return services;
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services) => services
            .AddMemoryCache()
            .AddSingleton(TimeProvider.System)
            .AddHttpClient()
            .AddJsonRepository<PlayerDataObject>(settings => settings.PlayersStorePath)
            .AddJsonRepository<RtpLocationEntity>(settings => settings.RtpLocationsStorePath)
            .AddJsonRepository<CountryDataObject>(settings => settings.CountriesStorePath)
            .AddJsonRepository<HomeDataObject>(settings => settings.HomesStorePath)
            .AddJsonRepository<ItemDataObject>(settings => settings.ItemsStorePath)
            .AddJsonRepository<WorldDataObject>(settings => settings.WorldsStorePath)
            .AddJsonRepository<ZoneDataObject>(settings => settings.ZonesStorePath)
            .AddJsonRepository<ZoneTypeDataObject>(settings => settings.ZoneTypesStorePath)
            .AddScoped<INuciApiClient>(serviceProvider =>
                new NuciApiClient(
                    serviceProvider
                        .GetRequiredService<UniversalNameGeneratorSettings>()
                        .BaseUrl))
            .AddScoped<IMobService, MobService>()
            .AddScoped<IServerStatusService, ServerStatusService>()
            .AddScoped<ICountryService, CountryService>()
            .AddScoped<IHomeService, HomeService>()
            .AddScoped<IItemService, ItemService>()
            .AddScoped<IWorldService, WorldService>()
            .AddScoped<IZoneTypeService, ZoneTypeService>()
            .AddScoped<IPlayerService, PlayerService>()
            .AddScoped<IRtpLocationService, RtpLocationService>()
            .AddScoped<IZoneService, ZoneService>()
            .AddScoped<IAccessControlService, AccessControlService>()
            .AddScoped<IAnonymisationServiceDetectorFactory, AnonymisationServiceDetectorFactory>()
            .AddSingleton<ISignIdGenerator, SignIdGenerator>()
            .AddSingleton<INuciTextNormaliser, NuciTextNormaliser>()
            .AddSingleton<INuciTextObfuscator, NuciTextObfuscator>()
            .AddScoped<ILogger, NuciLogger>();

        private static IServiceCollection AddJsonRepository<TDataObject>(
            this IServiceCollection services,
            Func<DataStoreSettings, string> storePathSelector)
            where TDataObject : NuciCraftEntityBase
            => services.AddScoped<IFileRepository<TDataObject>>(
                serviceProvider => new OptimisticJsonRepository<TDataObject>(
                    storePathSelector(
                        serviceProvider.GetRequiredService<IOptionsMonitor<DataStoreSettings>>().CurrentValue)));
    }
}
