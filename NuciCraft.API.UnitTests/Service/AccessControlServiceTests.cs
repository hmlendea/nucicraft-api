using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using NuciLog.Core;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
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

        private readonly StubHttpMessageHandler geolocationHandler =
            StubHttpMessageHandler.ReturningJson(AllowedCountryGeolocationJson);
        private readonly StubHttpMessageHandler anonymisationHandler =
            StubHttpMessageHandler.ReturningJson(AnonymisationServiceNotDetectedJson);

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
                MinimumUsernameLength = 4,
                GeolocationApiUrl = "https://geo.test/",
                AnonymisationServiceDetection = new()
                {
                    ApiUrl = "https://anonymisation.test/check",
                    ApiLicense = "license/value",
                    IsCacheEnabled = false,
                    IsEnabled = true
                }
            };

            service = new AccessControlService(
                settings,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocationHandler, anonymisationHandler)));
        }

        [Test]
        public void GivenANullRequest_WhenCheckingTheIp_ThenAnExceptionIsThrown()
            => Assert.That(() => service.CheckIp(null), Throws.ArgumentNullException);

        [Test]
        public void GivenAnIpFromAnAllowedCountryWithoutAnAnonymisationService_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public void GivenAnIpFromADeniedCountry_WhenCheckingTheIp_ThenTheResultIsDenied()
        {
            StubHttpMessageHandler deniedGeoHandler = StubHttpMessageHandler.ReturningJson(DeniedCountryGeolocationJson);

            service = CreateServiceWithHandlers(deniedGeoHandler, anonymisationHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "1.2.3.4" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
            Assert.That(result.CountryCode, Is.EqualTo("CN"));
        }

        [TestCase("cn")]
        [TestCase("Ru")]
        public void GivenADeniedCountryCodeWithDifferentCasing_WhenCheckingTheIp_ThenTheResultIsDenied(
            string countryCode)
        {
            string json = $$"""{"status":"success","countryCode":"{{countryCode}}","country":"Somewhere"}""";
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.ReturningJson(json);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "1.2.3.4" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted from this country."));
        }

        [Test]
        public void GivenAnIpDetectedAsAnAnonymisationService_WhenCheckingTheIp_ThenTheResultIsDenied()
        {
            StubHttpMessageHandler detectedHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceDetectedJson);

            service = CreateServiceWithHandlers(geolocationHandler, detectedHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.DeniedReason, Is.EqualTo("Access is not permitted via any anonymisation service."));
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public void GivenAnAnonymisationServiceCheckWithoutSuccess_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            StubHttpMessageHandler failedCheckHandler = StubHttpMessageHandler.ReturningJson(AnonymisationServiceCheckFailedJson);

            service = CreateServiceWithHandlers(geolocationHandler, failedCheckHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
        }

        [Test]
        public void GivenADisabledAnonymisationServiceDetection_WhenCheckingTheIp_ThenTheAnonymisationApiIsNotCalled()
        {
            settings.AnonymisationServiceDetection.IsEnabled = false;
            service = CreateServiceWithHandlers(geolocationHandler, anonymisationHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(anonymisationHandler.LastRequest, Is.Null);
        }

        [Test]
        public void GivenAFailingAnonymisationServiceApi_WhenCheckingTheIp_ThenAnAllowedResultIsReturned()
        {
            StubHttpMessageHandler failingHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.InternalServerError);

            service = CreateServiceWithHandlers(geolocationHandler, failingHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public void GivenAFailingAnonymisationServiceApi_WhenCheckingTheIp_ThenTheFailureIsLogged()
        {
            StubHttpMessageHandler failingHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.InternalServerError);

            service = CreateServiceWithHandlers(geolocationHandler, failingHandler);
            service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(
                logger.Invocations.Count(invocation => invocation.Method.Name == nameof(ILogger.Error)),
                Is.GreaterThanOrEqualTo(1));
        }

        [TestCase("""{"status":"fail","countryCode":"","country":""}""")]
        [TestCase("""{"status":"FAIL","countryCode":"US","country":"United States"}""")]
        public void GivenAFailingGeolocationLookup_WhenCheckingTheIp_ThenAnAllowedResultIsReturned(
            string geolocationJson)
        {
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.ReturningJson(geolocationJson);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            IpCheckResult result = service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.DeniedReason, Is.Null);
            Assert.That(result.CountryCode, Is.Null);
        }

        [Test]
        public void GivenAFailingGeolocationApi_WhenCheckingTheIp_ThenAnExceptionIsThrown()
        {
            StubHttpMessageHandler geoHandler = StubHttpMessageHandler.FailingWith(HttpStatusCode.ServiceUnavailable);

            service = CreateServiceWithHandlers(geoHandler, anonymisationHandler);

            Assert.That(
                () => service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" }),
                Throws.InstanceOf<HttpRequestException>());
        }

        [Test]
        public void GivenAnEnablingAnonymisationServiceDetection_WhenCheckingTheIp_ThenTheRequestContainsTheIpAddressAndSettings()
        {
            service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("ip=8.8.8.8"));
            Assert.That(requestedUrl, Does.Contain("license=license%2Fvalue"));
            Assert.That(requestedUrl, Does.Contain("cache=false"));
        }

        [Test]
        public void GivenAnEnabledAnonymisationServiceDetectionCache_WhenCheckingTheIp_ThenTheRequestAsksForTheCache()
        {
            settings.AnonymisationServiceDetection.IsCacheEnabled = true;
            service = CreateServiceWithHandlers(geolocationHandler, anonymisationHandler);

            service.CheckIp(new CheckIpRequest { IpAddress = "8.8.8.8" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("cache=true"));
        }

        [Test]
        public void GivenAnIpWithSpecialCharacters_WhenCheckingTheIp_ThenTheIpAddressIsEscaped()
        {
            service.CheckIp(new CheckIpRequest { IpAddress = "1.2.3.4&x=1" });

            string requestedUrl = GetRequestedUrl(anonymisationHandler);

            Assert.That(requestedUrl, Does.Contain("ip=1.2.3.4%26x%3D1"));
        }

        private static string GetRequestedUrl(StubHttpMessageHandler handler)
        {
            if (handler.LastRequest?.RequestUri?.ToString() is string url)
            {
                return url;
            }

            return string.Empty;
        }

        private AccessControlService CreateServiceWithHandlers(
            StubHttpMessageHandler geolocation,
            StubHttpMessageHandler anonymisationDetection)
            => new(
                settings,
                logger.Object,
                new HttpClient(new RoutingHandler(geolocation, anonymisationDetection)));
    }
}