using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NuciDAL.Repositories;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Anonymisation;

using NuciLog.Core;

using NUnit.Framework;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class AccessControlServiceUsernameTests
    {
        private Mock<ILogger> logger;
        private Mock<IAnonymisationServiceDetectorFactory> factory;
        private Mock<IFileRepository<PlayerDataObject>> playerRepository;
        private AccessControlSettings settings;
        private AccessControlService service;

        [SetUp]
        public void SetUp()
        {
            logger = new Mock<ILogger>();
            factory = new Mock<IAnonymisationServiceDetectorFactory>();
            playerRepository = new Mock<IFileRepository<PlayerDataObject>>();
            playerRepository.Setup(repository => repository.GetAll()).Returns([]);
            settings = new AccessControlSettings
            {
                DeniedCountries = ["CN", "RU"],
                DeniedUsernamePatterns = ["(official)", "[Ss]canner$", "^[Pp]layer$"],
                MinimumUsernameLength = 4,
                GeolocationApiUrl = "https://geo.test/",
                AnonymisationServiceDetection = new()
                {
                    Provider = "funkemunky",
                    IsCacheEnabled = false,
                    IsEnabled = true
                }
            };
            IAnonymisationServiceDetector detector = new FunkeMunkyAnonymisationServiceDetector(
                settings,
                new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"success":true,"proxy":false}""") })));
            factory.Setup(f => f.Create()).Returns(detector);
            service = new AccessControlService(settings, factory.Object, playerRepository.Object, logger.Object);
        }

        [Test]
        public async Task GivenANullRequest_WhenCheckingTheUsername_ThenAnExceptionIsThrown()
            => Assert.That(() => service.CheckUsernameAsync(null), Throws.ArgumentNullException);

        [Test]
        public async Task GivenANonEmptyUsername_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned()
        {
            CheckUsernameRequest request = new() { Username = "NuciPlayer" };

            Assert.That((await service.CheckUsernameAsync(request)).IsAllowed, Is.True);
            Assert.That((await service.CheckUsernameAsync(request)).DeniedReason, Is.Null);
        }

        [TestCase("asdf")]
        [TestCase("sdfg")]
        [TestCase("dfgh")]
        public async Task GivenAUsernameAtTheMinimumLength_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That((await service.CheckUsernameAsync(request)).IsAllowed, Is.True);
        }

        [TestCase("asd")]
        [TestCase("dfg")]
        [TestCase("z")]
        public async Task GivenATooShortUsername_WhenCheckingTheUsername_ThenTheResultIsDenied(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That((await service.CheckUsernameAsync(request)).IsAllowed, Is.False);
            Assert.That((await service.CheckUsernameAsync(request)).DeniedReason, Is.EqualTo("This username is not permitted."));
        }

        [TestCase("myofficialname")]
        [TestCase("official_org")]
        [TestCase("NuciScanner")]
        [TestCase("Player")]
        [TestCase("player")]
        public async Task GivenAUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheResultIsDenied(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That((await service.CheckUsernameAsync(request)).IsAllowed, Is.False);
            Assert.That((await service.CheckUsernameAsync(request)).DeniedReason, Is.EqualTo("This username is not permitted."));
        }

        [TestCase("Official")]
        [TestCase("SCANNER")]
        public async Task GivenAUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That((await service.CheckUsernameAsync(request)).IsAllowed, Is.True);
        }
    }
}
