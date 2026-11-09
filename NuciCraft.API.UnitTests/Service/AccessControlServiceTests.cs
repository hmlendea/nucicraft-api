using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NuciDAL.Repositories;

using NuciLog.Core;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Anonymisation;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class AccessControlServiceTests
    {
        private static string AllowedCountryGeolocationJson
            => """{"status":"success","countryCode":"US","country":"United States"}""";
        private static string DeniedCountryGeolocationJson
            => """{"status":"success","countryCode":"CN","country":"China"}""";
        private static string AnonymisationServiceDetectedJson
            => """{"success":true,"proxy":true}""";
        private static string AnonymisationServiceNotDetectedJson
            => """{"success":true,"proxy":false}""";
        private static string AnonymisationServiceCheckFailedJson
            => """{"success":false,"proxy":false}""";

        private StubHttpMessageHandler geolocationHandler = null!;
        private StubHttpMessageHandler anonymisationHandler = null!;

        private Mock<ILogger> logger;
        private Mock<IAnonymisationServiceDetectorFactory> factory;
        private Mock<IFileRepository<PlayerDataObject>> playerRepository;
        private AccessControlSettings settings;
        private ServerSettings serverSettings;
        private AccessControlService service;

        [SetUp]
        public void SetUp()
        {
            geolocationHandler = StubHttpMessageHandler.ReturningJson(AllowedCountryGeolocationJson);
            anonymisationHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceNotDetectedJson);
            logger = new Mock<ILogger>();
            factory = new Mock<IAnonymisationServiceDetectorFactory>();
            playerRepository = new Mock<IFileRepository<PlayerDataObject>>();
            playerRepository.Setup(repository => repository.GetAll()).Returns([]);
            settings = new AccessControlSettings
            {
                DeniedCountries = ["CN", "RU"],
                MinimumUsernameLength = 4,
                GeolocationApiUrl = "https://geo.test/",
                AnonymisationServiceDetection = new()
                {
                    Provider = "funkemunky",
                    IsCacheEnabled = false,
                    IsEnabled = true
                }
            };
            serverSettings = new ServerSettings
            {
                Name = "NuciCraft"
            };

            IAnonymisationServiceDetector detector = new FunkeMunkyAnonymisationServiceDetector(
                settings,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));
            factory.Setup(f => f.Create()).Returns(detector);

            service = new AccessControlService(
                settings,
                serverSettings,
                factory.Object,
                playerRepository.Object,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));
        }

        [Test]
        public async Task GivenANullRequest_WhenCheckingTheIp_ThenAnExceptionIsThrown()
            => Assert.That(() => service.CheckIpAsync(null), Throws.ArgumentNullException);

        [Test]
        public async Task GivenAnIpFromAnAllowedCountryWithoutAnAnonymisationService_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenAnIpFromADeniedCountry_WhenCheckingTheIp_ThenTheResultIsDenied()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "1.2.3.4" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
            Assert.That(result.CountryCode, Is.EqualTo("CN"));
        }

        [TestCase("cn")]
        [TestCase("Ru")]
        public async Task GivenADeniedCountryCodeWithDifferentCasing_WhenCheckingTheIp_ThenTheResultIsDenied(
            string countryCode)
        {
            string json = $$"""{"status":"success","countryCode":"{{countryCode}}","country":"Somewhere"}""";
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.ReturningJson(json);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "1.2.3.4" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [Test]
        public async Task GivenAnIpDetectedAsAnAnonymisationService_WhenCheckingTheIp_ThenTheResultIsDenied()
        {
            StubHttpMessageHandler detectedHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceDetectedJson);

            service = CreateServiceWithHandlers(geolocationHandler, detectedHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted via any anonymisation service."));
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenAnAnonymisationServiceCheckWithoutSuccess_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            StubHttpMessageHandler failedCheckHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceCheckFailedJson);

            service = CreateServiceWithHandlers(geolocationHandler, failedCheckHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [Test]
        public async Task GivenADisabledAnonymisationServiceDetection_WhenCheckingTheIp_ThenTheAnonymisationApiIsNotCalled()
        {
            settings.AnonymisationServiceDetection.IsEnabled = false;
            service = CreateServiceWithHandlers(geolocationHandler, anonymisationHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(anonymisationHandler.LastRequest, Is.Null);
        }

        [Test]
        public async Task GivenAFailingAnonymisationServiceApi_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            StubHttpMessageHandler failingHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.InternalServerError);

            service = CreateServiceWithHandlers(geolocationHandler, failingHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenAFailingAnonymisationServiceApi_WhenCheckingTheIp_ThenTheFailureIsLogged()
        {
            StubHttpMessageHandler failingHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.InternalServerError);

            service = CreateServiceWithHandlers(geolocationHandler, failingHandler);
            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(
                logger.Invocations.Count(invocation => invocation.Method.Name == nameof(ILogger.Error)),
                Is.GreaterThanOrEqualTo(1));
        }

        [TestCase("""{"status":"fail","countryCode":"","country":""}""")]
        [TestCase("""{"status":"FAIL","countryCode":"US","country":"United States"}""")]
        public async Task GivenAFailingGeolocationLookup_WhenCheckingTheIp_ThenAnAllowedResultIsReturned(
            string geolocationJson)
        {
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.ReturningJson(geolocationJson);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
            Assert.That(result.CountryCode, Is.Null);
        }

        [Test]
        public async Task GivenAFailingGeolocationApi_WhenCheckingTheIp_ThenAnExceptionIsThrown()
        {
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.ServiceUnavailable);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            Assert.That(
                () => service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" }),
                Throws.InstanceOf<HttpRequestException>());
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingADeniedIp_ThenTheResultIsAllowed()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("IlarionPintilie", false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = "IlarionPintilie"
            });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingTheIp_ThenTheExternalChecksAreSkipped()
        {
            StubHttpMessageHandler geolocationDetection = StubHttpMessageHandler.ReturningJson(AllowedCountryGeolocationJson);
            StubHttpMessageHandler anonymisationDetection = StubHttpMessageHandler.ReturningJson(AnonymisationServiceNotDetectedJson);

            service = CreateServiceWithHandlers(geolocationDetection, anonymisationDetection);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("IlarionPintilie", false)]);

            await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = "IlarionPintilie"
            });

            Assert.That(geolocationDetection.LastRequest, Is.Null);
            Assert.That(anonymisationDetection.LastRequest, Is.Null);
        }

        [Test]
        public async Task GivenARegisteredBannedUsername_WhenCheckingADeniedIp_ThenTheIpIsDeniedForTheBannedUser()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("GrumpyCat", true)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = "GrumpyCat"
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is associated with a user banned on NuciCraft"));
        }

        [Test]
        public async Task GivenAnUnregisteredUsername_WhenCheckingADeniedIp_ThenTheRegularChecksApply()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("IlarionPintilie", false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = "UnknownPlayer"
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [TestCase("IlarionPintilie")]
        [TestCase("Angetenar")]
        public async Task GivenADisabledRegisteredPlayerExemption_WhenCheckingADeniedIp_ThenTheRegularChecksApply(
            string username)
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            settings.IsRegisteredPlayerExemptionEnabled = false;
            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer("IlarionPintilie", false), BuildPlayer("Angetenar", false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = username
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [Test]
        public async Task GivenAWhitespaceUsername_WhenCheckingADeniedIp_ThenTheRegularChecksApply()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("   ", false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = "   "
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameWithDifferentCasing_WhenCheckingADeniedIp_ThenTheRegularChecksApply()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository.Setup(repository => repository.GetAll()).Returns([BuildPlayer("IlarionPintilie", false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "1.2.3.4",
                Username = "ilarionpintilie"
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheResultIsDenied()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is banned on NuciCraft"));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheExternalChecksAreSkipped()
        {
            StubHttpMessageHandler geolocationDetection = StubHttpMessageHandler.ReturningJson(AllowedCountryGeolocationJson);
            StubHttpMessageHandler anonymisationDetection = StubHttpMessageHandler.ReturningJson(AnonymisationServiceNotDetectedJson);

            service = CreateServiceWithHandlers(geolocationDetection, anonymisationDetection);
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(geolocationDetection.LastRequest, Is.Null);
            Assert.That(anonymisationDetection.LastRequest, Is.Null);
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenThePlayerStoreIsReadOnce()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            playerRepository.Verify(repository => repository.GetAll(), Times.Once);
        }

        [Test]
        public async Task GivenADeniedCountryAndAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheBannedPlayerReasonIsReturned()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is banned on NuciCraft"));
            Assert.That(result.CountryCode, Is.Null);
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayerAndAnUnregisteredUsername_WhenCheckingTheIp_ThenTheBannedIpAddressMessageIsReturned()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = "DummyUser"
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is associated with a user banned on NuciCraft"));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayerAndABannedUsername_WhenCheckingTheIp_ThenTheBannedUserMessageIsReturned()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = "GrumpyCat"
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is associated with a user banned on NuciCraft"));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameAndAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheResultIsAllowed()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns(
                [
                    BuildBannedPlayer("GrumpyCat", "8.8.8.8"),
                    BuildPlayer("IlarionPintilie", false)
                ]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = "IlarionPintilie"
            });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [TestCase("8.8.4.4")]
        [TestCase("8.8.8")]
        public async Task GivenAnIpAddressThatDoesNotMatchTheBannedPlayer_WhenCheckingTheIp_ThenTheRegularChecksApply(
            string ipAddress)
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = ipAddress });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenANonBannedPlayerOnTheIpAddress_WhenCheckingTheIp_ThenTheResultIsAllowed()
        {
            PlayerDataObject player = BuildPlayer("IlarionPintilie", false);
            player.LastIpAddress = "8.8.8.8";

            playerRepository.Setup(repository => repository.GetAll()).Returns([player]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenABannedPlayerWithAnotherIpAddress_WhenCheckingTheIp_ThenTheRegularChecksApply()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "1.1.1.1")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheServerNameIsUsedInTheDenialMessage()
        {
            serverSettings.Name = "Nucilandia";
            service = CreateServiceWithHandlers(geolocationHandler, anonymisationHandler);
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildBannedPlayer("GrumpyCat", "8.8.8.8")]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.DeniedReason, Is.EqualTo("This IP address is banned on Nucilandia"));
        }

        private AccessControlService CreateServiceWithHandlers(
            StubHttpMessageHandler geolocation,
            StubHttpMessageHandler anonymisationDetection)
        {
            IAnonymisationServiceDetector detector = new FunkeMunkyAnonymisationServiceDetector(
                settings,
                new HttpClient(new RoutingHandler(geolocation, anonymisationDetection)));
            factory.Setup(f => f.Create()).Returns(detector);
            return new AccessControlService(
                settings,
                serverSettings,
                factory.Object,
                playerRepository.Object,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocation, anonymisationDetection)));
        }

        [Test]
        public async Task GivenAnEnablingAnonymisationServiceDetection_WhenCheckingTheIp_ThenTheRequestContainsTheIpAddressAndSettings()
        {
            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("ip=8.8.8.8"));
            Assert.That(requestedUrl, Does.Contain("license=none"));
            Assert.That(requestedUrl, Does.Contain("cache=false"));
        }

        [Test]
        public async Task GivenAnEnabledAnonymisationServiceDetectionCache_WhenCheckingTheIp_ThenTheRequestDoesNotAskForTheUpstreamCache()
        {
            settings.AnonymisationServiceDetection.IsCacheEnabled = true;
            IAnonymisationServiceDetector detector = new FunkeMunkyAnonymisationServiceDetector(
                settings,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));
            factory.Setup(f => f.Create()).Returns(detector);
            service = new AccessControlService(
                settings,
                serverSettings,
                factory.Object,
                playerRepository.Object,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));

            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("cache=false"));
        }

        [Test]
        public async Task GivenAnIpWithSpecialCharacters_WhenCheckingTheIp_ThenTheIpAddressIsEscaped()
        {
            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "1.2.3.4&x=1" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("ip=1.2.3.4%26x%3D1"));
        }

        [Test]
        public async Task GivenProxyCheckProvider_WhenCheckingTheIp_ThenTheRequestUsesProxyCheckUrl()
        {
            settings.AnonymisationServiceDetection.Provider = "proxycheck";
            IAnonymisationServiceDetector detector = new ProxyCheckAnonymisationServiceDetector(
                settings,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));
            factory.Setup(f => f.Create()).Returns(detector);
            service = new AccessControlService(
                settings,
                serverSettings,
                factory.Object,
                playerRepository.Object,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));

            await service.CheckIpAsync(new CheckIpRequest { IpAddress = "8.8.8.8" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("proxycheck.io/v2/8.8.8.8"));
            Assert.That(requestedUrl, Does.Contain("vpn=1"));
            Assert.That(requestedUrl, Does.Contain("asn=1"));
            Assert.That(requestedUrl, Does.Contain("risk=1"));
            Assert.That(requestedUrl, Does.Contain("cache=false"));
        }

        private static string GetRequestedUrl(StubHttpMessageHandler handler)
        {
            if (handler.LastRequest?.RequestUri?.ToString() is string url)
            {
                return url;
            }

            return string.Empty;
        }

        private static PlayerDataObject BuildPlayer(string username, bool isBanned) => new()
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            IsBanned = isBanned
        };

        private static PlayerDataObject BuildBannedPlayer(
            string username,
            string lastIpAddress) => new()
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                IsBanned = true,
                LastIpAddress = lastIpAddress
            };
    }
}
