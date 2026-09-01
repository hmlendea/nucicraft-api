using System;
using System.Diagnostics;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using NuciDAL.Repositories;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.UnitTests
{
    [TestFixture]
    public sealed class StartupTests
    {
        private string storeDirectory;
        private IConfiguration configuration;
        private Startup startup;
        private ServiceProvider serviceProvider;
        private Mock<IFileRepository<PlayerDataObject>> playerRepositoryMock;
        private Mock<IFileRepository<RtpLocationEntity>> rtpLocationRepositoryMock;
        private Mock<IFileRepository<CountryDataObject>> countryRepositoryMock;
        private Mock<IFileRepository<WorldDataObject>> worldRepositoryMock;
        private Mock<IFileRepository<ZoneDataObject>> zoneRepositoryMock;
        private Mock<IFileRepository<ZoneTypeDataObject>> zoneTypeRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            storeDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(StartupTests),
                Guid.NewGuid().ToString());
            configuration = TestConfigurationFactory.Build(storeDirectory);
            startup = new Startup(configuration);
            playerRepositoryMock = new Mock<IFileRepository<PlayerDataObject>>();
            rtpLocationRepositoryMock = new Mock<IFileRepository<RtpLocationEntity>>();
            countryRepositoryMock = new Mock<IFileRepository<CountryDataObject>>();
            worldRepositoryMock = new Mock<IFileRepository<WorldDataObject>>();
            zoneRepositoryMock = new Mock<IFileRepository<ZoneDataObject>>();
            zoneTypeRepositoryMock = new Mock<IFileRepository<ZoneTypeDataObject>>();
            playerRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
            rtpLocationRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
            countryRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
            worldRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
            zoneRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
            zoneTypeRepositoryMock.Setup(repository => repository.GetAll()).Returns([]);
        }

        [TearDown]
        public void TearDown()
        {
            serviceProvider?.Dispose();

            if (Directory.Exists(storeDirectory))
            {
                Directory.Delete(storeDirectory, true);
            }
        }

        [Test]
        public void GivenConfiguration_WhenCreatingStartup_ThenTheConfigurationIsRetained()
            => Assert.That(startup.Configuration, Is.SameAs(configuration));

        [Test]
        public void GivenAServiceCollection_WhenConfiguringServices_ThenControllersAndCustomServicesAreRegistered()
        {
            ServiceCollection services = new();

            startup.ConfigureServices(services);

            using ServiceProvider configuredServiceProvider = services.BuildServiceProvider();
            Assert.That(configuredServiceProvider.GetRequiredService<DataStoreSettings>(), Is.Not.Null);
            Assert.That(configuredServiceProvider.GetRequiredService<SecuritySettings>(), Is.Not.Null);
        }

        [Test]
        public void GivenDevelopmentAndProductionEnvironments_WhenConfiguringThePipeline_ThenStoresArePreparedAndRepositoriesAreLoaded()
        {
            DataStoreSettings dataStoreSettings = TestConfigurationFactory.BuildDataStoreSettings(storeDirectory);
            serviceProvider = BuildServiceProvider(dataStoreSettings);
            Mock<IWebHostEnvironment> developmentEnvironmentMock = BuildEnvironmentMock(Environments.Development);
            Mock<IWebHostEnvironment> productionEnvironmentMock = BuildEnvironmentMock(Environments.Production);

            startup.Configure(
                new ApplicationBuilder(serviceProvider),
                developmentEnvironmentMock.Object);
            startup.Configure(
                new ApplicationBuilder(serviceProvider),
                productionEnvironmentMock.Object);

            Assert.That(File.Exists(dataStoreSettings.PlayersStorePath));
            Assert.That(File.Exists(dataStoreSettings.RtpLocationsStorePath));
            Assert.That(File.Exists(dataStoreSettings.CountriesStorePath));
            Assert.That(File.Exists(dataStoreSettings.WorldsStorePath));
            Assert.That(File.Exists(dataStoreSettings.ZonesStorePath));
            Assert.That(File.Exists(dataStoreSettings.ZoneTypesStorePath));
            playerRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
            rtpLocationRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
            countryRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
            worldRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
            zoneRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
            zoneTypeRepositoryMock.Verify(repository => repository.GetAll(), Times.Exactly(2));
        }

        private ServiceProvider BuildServiceProvider(DataStoreSettings dataStoreSettings)
        {
            ServiceCollection services = new();
            startup.ConfigureServices(services);
            services.AddLogging();
            services.AddSingleton(new DiagnosticListener(nameof(StartupTests)));
            services.AddSingleton(dataStoreSettings);
            services.AddSingleton(playerRepositoryMock.Object);
            services.AddSingleton(rtpLocationRepositoryMock.Object);
            services.AddSingleton(countryRepositoryMock.Object);
            services.AddSingleton(worldRepositoryMock.Object);
            services.AddSingleton(zoneRepositoryMock.Object);
            services.AddSingleton(zoneTypeRepositoryMock.Object);

            return services.BuildServiceProvider();
        }

        private static Mock<IWebHostEnvironment> BuildEnvironmentMock(string environmentName)
        {
            Mock<IWebHostEnvironment> environmentMock = new();
            environmentMock
                .SetupGet(environment => environment.EnvironmentName)
                .Returns(environmentName);

            return environmentMock;
        }
    }
}