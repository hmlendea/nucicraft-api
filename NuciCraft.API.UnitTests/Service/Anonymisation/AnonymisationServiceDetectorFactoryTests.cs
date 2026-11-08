using System;
using System.Net.Http;
using System.Threading;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NuciLog.Core;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service.Anonymisation;

namespace NuciCraft.API.UnitTests.Service.Anonymisation
{
    [TestFixture]
    public sealed class AnonymisationServiceDetectorFactoryTests
    {
        private Mock<ILogger> loggerMock;
        private ServiceProvider serviceProvider;
        private AccessControlSettings settings;

        [SetUp]
        public void SetUp()
        {
            loggerMock = new Mock<ILogger>();
            settings = new AccessControlSettings
            {
                AnonymisationServiceDetection = new()
                {
                    IsEnabled = true,
                    IsCacheEnabled = true
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            serviceProvider?.Dispose();
        }

        [TestCase("proxycheck")]
        [TestCase("ProxyCheck")]
        [TestCase("funkemunky")]
        [TestCase("FunkeMunky")]
        public void GivenASupportedProvider_WhenCreatingADetector_ThenACachedDetectorIsCreated(string provider)
        {
            settings.AnonymisationServiceDetection.Provider = provider;
            IAnonymisationServiceDetectorFactory factory = CreateFactory();

            IAnonymisationServiceDetector detector = factory.Create();

            Assert.That(detector, Is.TypeOf<CachedAnonymisationServiceDetector>());
        }

        [TestCase("unsupported")]
        [TestCase("")]
        [TestCase("proxy-check")]
        public void GivenAnUnsupportedProvider_WhenCreatingADetector_ThenANotSupportedExceptionIsThrown(string provider)
        {
            settings.AnonymisationServiceDetection.Provider = provider;
            IAnonymisationServiceDetectorFactory factory = CreateFactory();

            Assert.That(
                () => factory.Create(),
                Throws.InstanceOf<NotSupportedException>());
        }

        [Test]
        public void GivenASupportedProvider_WhenCreatingADetector_ThenTheDetectorIsCached()
        {
            settings.AnonymisationServiceDetection.Provider = "proxycheck";
            IAnonymisationServiceDetectorFactory factory = CreateFactory();

            IAnonymisationServiceDetector detector = factory.Create();

            Assert.That(detector, Is.TypeOf<CachedAnonymisationServiceDetector>());
        }

        private IAnonymisationServiceDetectorFactory CreateFactory()
        {
            ServiceCollection services = new();
            services
                .AddSingleton<ILogger>(loggerMock.Object)
                .AddSingleton(settings)
                .AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()))
                .AddSingleton(TimeProvider.System)
                .AddSingleton(new HttpClient());

            serviceProvider = services.BuildServiceProvider();

            return new AnonymisationServiceDetectorFactory(
                settings,
                serviceProvider,
                loggerMock.Object);
        }
    }
}
