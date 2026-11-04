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
        public void GivenANullRequest_WhenCheckingTheUsername_ThenAnExceptionIsThrown()
            => Assert.That(() => service.CheckUsername(null), Throws.ArgumentNullException);

        [Test]
        public void GivenANonEmptyUsername_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned()
        {
            CheckUsernameRequest request = new() { Username = "NuciPlayer" };

            Assert.That(service.CheckUsername(request).IsAllowed, Is.True);
            Assert.That(service.CheckUsername(request).DeniedReason, Is.Null);
        }

        [TestCase("asdf")]
        [TestCase("sdfg")]
        [TestCase("dfgh")]
        public void GivenAUsernameAtTheMinimumLength_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That(service.CheckUsername(request).IsAllowed, Is.True);
        }

        [TestCase("asd")]
        [TestCase("dfg")]
        [TestCase("z")]
        public void GivenATooShortUsername_WhenCheckingTheUsername_ThenTheResultIsDenied(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That(service.CheckUsername(request).IsAllowed, Is.False);
            Assert.That(service.CheckUsername(request).DeniedReason, Is.EqualTo("This username is not permitted."));
        }

        [TestCase("myofficialname")]
        [TestCase("official_org")]
        [TestCase("NuciScanner")]
        [TestCase("Player")]
        [TestCase("player")]
        public void GivenAUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheResultIsDenied(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That(service.CheckUsername(request).IsAllowed, Is.False);
            Assert.That(service.CheckUsername(request).DeniedReason, Is.EqualTo("This username is not permitted."));
        }

        [TestCase("Official")]
        [TestCase("SCANNER")]
        public void GivenAUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenAnAllowedResultIsReturned(
            string username)
        {
            CheckUsernameRequest request = new() { Username = username };

            Assert.That(service.CheckUsername(request).IsAllowed, Is.True);
        }
    }
}
