using System;
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
    public sealed class ServiceLifetimeTests
    {
        private string storeDirectory;
        private IConfiguration configuration;
        private ServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            storeDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(ServiceLifetimeTests),
                Guid.NewGuid().ToString());
            configuration = TestConfigurationFactory.Build(storeDirectory);
            ServiceCollection services = new();
            services
                .AddConfigurations(configuration)
                .AddCustomServices();
            serviceProvider = services.BuildServiceProvider();
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
        public void GivenConfiguredServices_WhenCreatingTwoScopes_ThenEachScopeResolvesDistinctScopedRepositories()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            IFileRepository<PlayerDataObject> repo1 = scope1.ServiceProvider.GetRequiredService<IFileRepository<PlayerDataObject>>();
            IFileRepository<PlayerDataObject> repo2 = scope2.ServiceProvider.GetRequiredService<IFileRepository<PlayerDataObject>>();

            Assert.That(repo1, Is.Not.SameAs(repo2));
        }

        [Test]
        public void GivenConfiguredServices_WhenCreatingTwoScopes_ThenEachScopeResolvesDistinctScopedServices()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            IMobService service1 = scope1.ServiceProvider.GetRequiredService<IMobService>();
            IMobService service2 = scope2.ServiceProvider.GetRequiredService<IMobService>();

            Assert.That(service1, Is.Not.SameAs(service2));
        }

        [Test]
        public void GivenConfiguredServices_WhenCreatingTwoScopes_ThenEachScopeResolvesDistinctNuciApiClients()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            INuciApiClient client1 = scope1.ServiceProvider.GetRequiredService<INuciApiClient>();
            INuciApiClient client2 = scope2.ServiceProvider.GetRequiredService<INuciApiClient>();

            Assert.That(client1, Is.Not.SameAs(client2));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingTwiceWithinSameScope_ThenSameScopedRepositoryInstanceIsReturned()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            IFileRepository<PlayerDataObject> repo1 = scope.ServiceProvider.GetRequiredService<IFileRepository<PlayerDataObject>>();
            IFileRepository<PlayerDataObject> repo2 = scope.ServiceProvider.GetRequiredService<IFileRepository<PlayerDataObject>>();

            Assert.That(repo1, Is.SameAs(repo2));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingTwiceWithinSameScope_ThenSameScopedServiceInstanceIsReturned()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            IMobService service1 = scope.ServiceProvider.GetRequiredService<IMobService>();
            IMobService service2 = scope.ServiceProvider.GetRequiredService<IMobService>();

            Assert.That(service1, Is.SameAs(service2));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingTwiceWithinSameScope_ThenSameNuciApiClientInstanceIsReturned()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            INuciApiClient client1 = scope.ServiceProvider.GetRequiredService<INuciApiClient>();
            INuciApiClient client2 = scope.ServiceProvider.GetRequiredService<INuciApiClient>();

            Assert.That(client1, Is.SameAs(client2));
        }

        [Test]
        public void GivenConfiguredServices_WhenCreatingTwoScopes_ThenSingletonServicesAreShared()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            INuciTextNormaliser normaliser1 = scope1.ServiceProvider.GetRequiredService<INuciTextNormaliser>();
            INuciTextNormaliser normaliser2 = scope2.ServiceProvider.GetRequiredService<INuciTextNormaliser>();

            Assert.That(normaliser1, Is.SameAs(normaliser2));
        }

        [Test]
        public void GivenConfiguredServices_WhenCreatingTwoScopes_ThenEachScopeResolvesDistinctLoggers()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            ILogger logger1 = scope1.ServiceProvider.GetRequiredService<ILogger>();
            ILogger logger2 = scope2.ServiceProvider.GetRequiredService<ILogger>();

            Assert.That(logger1, Is.Not.SameAs(logger2));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingTwiceWithinSameScope_ThenSameLoggerInstanceIsReturned()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            ILogger logger1 = scope.ServiceProvider.GetRequiredService<ILogger>();
            ILogger logger2 = scope.ServiceProvider.GetRequiredService<ILogger>();

            Assert.That(logger1, Is.SameAs(logger2));
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingHttpClient_ThenManagedClientIsAvailable()
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            System.Net.Http.HttpClient httpClient = scope.ServiceProvider.GetRequiredService<System.Net.Http.HttpClient>();

            Assert.That(httpClient, Is.Not.Null);
        }

        [Test]
        public void GivenConfiguredServices_WhenResolvingHttpClientInDifferentScopes_ThenManagedClientIsAvailableInEachScope()
        {
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();

            System.Net.Http.HttpClient httpClient1 = scope1.ServiceProvider.GetRequiredService<System.Net.Http.HttpClient>();
            System.Net.Http.HttpClient httpClient2 = scope2.ServiceProvider.GetRequiredService<System.Net.Http.HttpClient>();

            Assert.That(httpClient1, Is.Not.Null);
            Assert.That(httpClient2, Is.Not.Null);
        }
    }
}