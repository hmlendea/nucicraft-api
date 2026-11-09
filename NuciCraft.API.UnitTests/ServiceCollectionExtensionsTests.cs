using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                serviceProvider.GetRequiredService<IOptionsMonitor<DataStoreSettings>>().CurrentValue.PlayersStorePath,
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
            Assert.That(
                serviceProvider.GetRequiredService<WebMapSettings>().BaseUrl,
                Is.EqualTo("https://mc.nucilandia.ro/nucicraft/webmap/"));
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
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<HomeDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<WorldDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<ZoneDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IFileRepository<ZoneTypeDataObject>>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciApiClient>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IMobService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IServerStatusService>(), Is.TypeOf<ServerStatusService>());
            Assert.That(serviceProvider.GetRequiredService<ICountryService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IHomeService>(), Is.TypeOf<HomeService>());
            Assert.That(serviceProvider.GetRequiredService<IWorldService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IZoneTypeService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IPlayerService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IRtpLocationService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<IZoneService>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciTextNormaliser>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<INuciTextObfuscator>(), Is.Not.Null);
            Assert.That(serviceProvider.GetRequiredService<ILogger>(), Is.Not.Null);
        }

        [Test]
        public void GivenTwoServiceCollectionsWithDifferentStorePaths_WhenResolvingTheirRepositories_ThenEachReadsItsOwnStore()
        {
            string testDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(ServiceCollectionExtensionsTests),
                Guid.NewGuid().ToString("N"));
            string firstStoreDirectory = Path.Combine(testDirectory, "first");
            string secondStoreDirectory = Path.Combine(testDirectory, "second");

            Directory.CreateDirectory(firstStoreDirectory);
            Directory.CreateDirectory(secondStoreDirectory);
            File.WriteAllText(
                Path.Combine(firstStoreDirectory, "items.json"),
                BuildItemsJson("first-item"));
            File.WriteAllText(
                Path.Combine(secondStoreDirectory, "items.json"),
                BuildItemsJson("second-item"));

            try
            {
                using ServiceProvider firstServiceProvider = BuildServiceProvider(firstStoreDirectory);
                using ServiceProvider secondServiceProvider = BuildServiceProvider(secondStoreDirectory);

                IEnumerable<ItemDataObject> firstItems = firstServiceProvider
                    .GetRequiredService<IFileRepository<ItemDataObject>>()
                    .GetAll();
                IEnumerable<ItemDataObject> secondItems = secondServiceProvider
                    .GetRequiredService<IFileRepository<ItemDataObject>>()
                    .GetAll();

                Assert.That(firstItems.Select(item => item.Id), Is.EqualTo(new[] { "first-item" }));
                Assert.That(secondItems.Select(item => item.Id), Is.EqualTo(new[] { "second-item" }));
            }
            finally
            {
                if (Directory.Exists(testDirectory))
                {
                    Directory.Delete(testDirectory, true);
                }
            }
        }

        private static ServiceProvider BuildServiceProvider(string storeDirectory)
        {
            ServiceCollection services = new();

            services
                .AddConfigurations(TestConfigurationFactory.Build(storeDirectory))
                .AddCustomServices();

            return services.BuildServiceProvider();
        }

        private static string BuildItemsJson(string itemIdentifier)
        {
            string template = """
                [
                  {
                    "id": "ITEM_IDENTIFIER",
                    "minecraftId": "minecraft:stone",
                    "nucicraftId": "nucicraft:stone",
                    "bukkitId": "STONE",
                    "signIds": [],
                    "createdDT": null,
                    "updatedDT": null,
                    "version": 0
                  }
                ]
                """;

            return template.Replace("ITEM_IDENTIFIER", itemIdentifier, StringComparison.Ordinal);
        }
    }
}
