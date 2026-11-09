using System;
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
    public sealed class AccessControlServiceRegistrationTests
    {
        private static string AllowedCountryGeolocationJson
            => """{"status":"success","countryCode":"US","country":"United States"}""";
        private static string AnonymisationServiceDetectedJson
            => """{"success":true,"proxy":true}""";
        private static string AnonymisationServiceNotDetectedJson
            => """{"success":true,"proxy":false}""";

        private static string BannedUsername => "NuciScanner";
        private static string RegisteredUsername => "IlarionPintilie";
        private static string UnregisteredUsername => "Angetenar";
        private static string BanReason => "Repeated rule violations";

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
                DeniedUsernamePatterns = ["[Oo]fficial", "^[Nn]uci[Ss]canner$"],
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

            service = CreateService(geolocationHandler, anonymisationHandler);
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingTheIp_ThenTheIpIsAllowedWithoutAnAnonymisationCheck()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(RegisteredUsername, false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = RegisteredUsername
            });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
            Assert.That(anonymisationHandler.LastRequest, Is.Null);
        }

        [Test]
        public async Task GivenARegisteredBannedUsername_WhenCheckingTheIp_ThenTheIpIsDeniedWithoutAnAnonymisationCheck()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(BannedUsername, true)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = BannedUsername
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Does.Contain("banned user"));
            Assert.That(anonymisationHandler.LastRequest, Is.Null);
        }

        [Test]
        public async Task GivenAnUnregisteredUsername_WhenCheckingTheIp_ThenTheAnonymisationCheckIsPerformed()
        {
            StubHttpMessageHandler detectedHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceDetectedJson);

            service = CreateService(geolocationHandler, detectedHandler);
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(RegisteredUsername, false)]);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = UnregisteredUsername
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted via any anonymisation service."));
            Assert.That(detectedHandler.LastRequest, Is.Not.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task GivenAnAbsentUsername_WhenCheckingTheIp_ThenTheAnonymisationCheckIsPerformed(
            string? username)
        {
            StubHttpMessageHandler detectedHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceDetectedJson);

            service = CreateService(geolocationHandler, detectedHandler);

            IpCheckResult result = await service.CheckIpAsync(new CheckIpRequest
            {
                IpAddress = "8.8.8.8",
                Username = username
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted via any anonymisation service."));
            Assert.That(detectedHandler.LastRequest, Is.Not.Null);
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsAllowed()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(BannedUsername, false)]);

            UsernameCheckResult result = await service.CheckUsernameAsync(new CheckUsernameRequest
            {
                Username = BannedUsername
            });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [Test]
        public async Task GivenARegisteredBannedUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDeniedWithTheBanReason()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(BannedUsername, true)]);

            UsernameCheckResult result = await service.CheckUsernameAsync(new CheckUsernameRequest
            {
                Username = BannedUsername
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Does.Contain("banned"));
            Assert.That(result.DeniedReason, Does.Contain(BanReason));
        }

        [Test]
        public async Task GivenAnUnregisteredUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDenied()
        {
            UsernameCheckResult result = await service.CheckUsernameAsync(new CheckUsernameRequest
            {
                Username = BannedUsername
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("This username is not permitted."));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsAllowed()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(RegisteredUsername, false)]);

            UsernameCheckResult result = await service.CheckUsernameAsync(new CheckUsernameRequest
            {
                Username = RegisteredUsername
            });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [Test]
        public async Task GivenARegisteredBannedUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDeniedWithTheBanReason()
        {
            playerRepository
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayer(RegisteredUsername, true)]);

            UsernameCheckResult result = await service.CheckUsernameAsync(new CheckUsernameRequest
            {
                Username = RegisteredUsername
            });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Does.Contain("banned"));
            Assert.That(result.DeniedReason, Does.Contain(BanReason));
        }

        private AccessControlService CreateService(
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

        private static PlayerDataObject BuildPlayer(string username, bool isBanned)
        {
            PlayerDataObject player = new()
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                IsBanned = isBanned
            };

            if (isBanned)
            {
                player.BannedReason = BanReason;
            }

            return player;
        }
    }
}
