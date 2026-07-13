using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        static DataStoreSettings dataStoreSettings;

        public static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            dataStoreSettings = new DataStoreSettings();
            SecuritySettings securitySettings = new();
            RtpLocationSettings rtpLocationSettings = new();

            configuration.Bind(nameof(dataStoreSettings), dataStoreSettings);
            configuration.Bind(nameof(securitySettings), securitySettings);
            configuration.Bind(nameof(rtpLocationSettings), rtpLocationSettings);

            return services
                .AddSingleton(dataStoreSettings)
                .AddSingleton(securitySettings)
                .AddSingleton(rtpLocationSettings)
                .AddNuciLoggerSettings(configuration);
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services) => services
            .AddSingleton<IFileRepository<PlayerEntity>>(x => new JsonRepository<PlayerEntity>(dataStoreSettings.PlayersStorePath))
            .AddSingleton<IFileRepository<RtpLocationEntity>>(x => new JsonRepository<RtpLocationEntity>(dataStoreSettings.RtpLocationsStorePath))
            .AddSingleton<IPlayerService, PlayerService>()
            .AddSingleton<IRtpLocationService, RtpLocationService>()
            .AddSingleton<INuciTextNormaliser, NuciTextNormaliser>()
            .AddSingleton<INuciTextObfuscator, NuciTextObfuscator>()
            .AddScoped<ILogger, NuciLogger>();
    }
}
