using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using NuciAPI.Client;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciText.Normalisation;
using NuciText.Obfuscation;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests
{
    [TestFixture]
    public sealed class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GivenConfigurationValues_WhenAddingConfigurations_ThenEverySettingsObjectIsRegistered()
        {
            string storeDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(ServiceCollectionExtensionsTests));
            IConfiguration configuration = TestConfigurationFactory.Build(storeDirectory);
            ServiceCollection services = new();

            services.AddConfigurations(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(
                serviceProvider.GetRequiredService<DataStoreSettings>().PlayersStorePath,
                Is.EqualTo(Path.Combine(storeDirectory, "players.json")));
            Assert.That(
                serviceProvider.GetRequiredService<SecuritySettings>().ApiKey,
                Is.EqualTo("NucileRullz!"));
            Assert.That(
                serviceProvider.GetRequiredService<RtpLocationSettings>().MinimumLocationDistance,
                Is.EqualTo(613));
            Assert.That(
                serviceProvider.GetRequiredService<UniversalNameGeneratorSettings>().BaseUrl,
                Is.EqualTo("https://dummy-url.com"));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingCustomServices_ThenEveryRegistrationCanBeCreated()
        {
            string storeDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(ServiceCollectionExtensionsTests));
            IConfiguration configuration = TestConfigurationFactory.Build(storeDirectory);
            ServiceCollection services = new();
            services
                .AddConfigurations(configuration)
                .AddCustomServices();

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<PlayerDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<RtpLocationEntity>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<CountryDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<WorldDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<ZoneDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<ZoneTypeDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciApiClient>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IMobService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<ICountryService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IWorldService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IZoneTypeService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IPlayerService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IRtpLocationService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IZoneService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciTextNormaliser>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciTextObfuscator>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<ILogger>(), Is.Not.Null);
        }
    }
}