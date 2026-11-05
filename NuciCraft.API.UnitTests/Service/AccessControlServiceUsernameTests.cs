using System.Threading.Tasks;

using Moq;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;

using NuciLog.Core;

using NUnit.Framework;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class AccessControlServiceUsernameTests
    {
        private Mock<ILogger> logger;
        private AccessControlSettings settings;
        private AccessControlService service;

        [SetUp]
        public void SetUp()
        {
            logger = new Mock<ILogger>();
            settings = new AccessControlSettings
            {
                DeniedCountries = ["CN", "RU"],
                DeniedUsernamePatterns = ["(official)", "[Ss]canner$", "^[Pp]layer$"],
                MinimumUsernameLength = 4,
                GeolocationApiUrl = "https://geo.test/",
                AnonymisationServiceDetection = new()
                {
                    ApiUrl = "https://anonymisation.test/check",
                    ApiLicense = "license/value",
                    IsCacheEnabled = false
                }
            };
            service = new AccessControlService(settings, logger.Object);
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
