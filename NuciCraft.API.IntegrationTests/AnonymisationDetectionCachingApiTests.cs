using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class AnonymisationDetectionCachingApiTests
    {
        private static string AllowedCountryGeolocationJson
            => """{"status":"success","countryCode":"US","country":"United States"}""";
        private static string AnonymisationServiceDetectedJson
            => """{"success":true,"proxy":true}""";
        private static string AnonymisationServiceNotDetectedJson
            => """{"success":true,"proxy":false}""";

        private static bool IsAnonymisationRequest(string url)
            => url.Contains("funkemunky.cc", StringComparison.Ordinal);

        private static bool IsGeolocationRequest(string url)
            => url.Contains("geo.test", StringComparison.Ordinal);

        private StubHttpMessageHandler httpMessageHandler = null!;
        private AccessControlApiTestHost testHost = null!;
        private HttpClient client = null!;

        [SetUp]
        public void SetUp()
        {
            httpMessageHandler = new StubHttpMessageHandler(
                StubHttpMessageHandler.Routing(
                    IsAnonymisationRequest,
                    IsGeolocationRequest,
                    _ => StubHttpMessageHandler.JsonResponse(AnonymisationServiceNotDetectedJson),
                    _ => StubHttpMessageHandler.JsonResponse(AllowedCountryGeolocationJson),
                    _ => StubHttpMessageHandler.FailingResponse(HttpStatusCode.NotFound)));

            testHost = new AccessControlApiTestHost(httpMessageHandler);
            client = testHost.CreateAuthorisedClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            testHost.Dispose();
        }

        [Test]
        public async Task GivenAnIpAddress_WhenCheckingItTwice_ThenTheDetectionRequestIsMadeOnce()
        {
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(1));
        }

        [Test]
        public async Task GivenDifferentIpAddresses_WhenCheckingThem_ThenEachAddressIsDetectedSeparately()
        {
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=1.1.1.1");

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(2));
        }

        [Test]
        public async Task GivenACachedDetectionResult_WhenCheckingTheIpAgain_ThenTheSameDecisionIsReturned()
        {
            HttpResponseMessage firstResponse = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string firstBody = await firstResponse.Content.ReadAsStringAsync();

            HttpResponseMessage secondResponse = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string secondBody = await secondResponse.Content.ReadAsStringAsync();

            Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondBody, Is.EqualTo(firstBody));
            Assert.That(secondBody, Does.Contain("\"isAllowed\":true"));
        }

        [Test]
        public async Task GivenADetectedAnonymisationService_WhenCheckingTheIpTwice_ThenAccessIsDeniedBothTimes()
        {
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson));

            HttpResponseMessage firstResponse = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string firstBody = await firstResponse.Content.ReadAsStringAsync();

            HttpResponseMessage secondResponse = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string secondBody = await secondResponse.Content.ReadAsStringAsync();

            Assert.That(firstBody, Does.Contain("\"isAllowed\":false"));
            Assert.That(firstBody, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
            Assert.That(secondBody, Does.Contain("\"isAllowed\":false"));
            Assert.That(secondBody, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
            Assert.That(CountAnonymisationRequests(), Is.EqualTo(1));
        }

        [Test]
        public async Task GivenAFailingDetectionServiceAndNoCache_WhenCheckingTheIp_ThenAccessIsPermitted()
        {
            SetDetectionResponse(StubHttpMessageHandler.FailingResponse(HttpStatusCode.ServiceUnavailable));

            HttpResponseMessage response = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenAnExpiredUndetectedResultAndASuccessfulRefresh_WhenCheckingTheIp_ThenTheNewResultIsCached()
        {
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            int requestsBeforeExpiry = CountAnonymisationRequests();

            testHost.TimeProvider.Advance(TimeSpan.FromHours(2));
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson));

            HttpResponseMessage response = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 1));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));

            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 1));
        }

        [Test]
        public async Task GivenAnExpiredDetectedResultAndASuccessfulRefresh_WhenCheckingTheIp_ThenTheNewResultIsCached()
        {
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson));
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            int requestsBeforeExpiry = CountAnonymisationRequests();

            testHost.TimeProvider.Advance(TimeSpan.FromHours(6));
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceNotDetectedJson));

            HttpResponseMessage response = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 1));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));

            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 1));
        }

        [Test]
        public async Task GivenAnExpiredResultAndAFailingRefresh_WhenCheckingTheIp_ThenTheExpiredResultIsUsedAsFallback()
        {
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson));
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            int requestsBeforeExpiry = CountAnonymisationRequests();

            testHost.TimeProvider.Advance(TimeSpan.FromHours(6));
            SetDetectionResponse(StubHttpMessageHandler.FailingResponse(HttpStatusCode.ServiceUnavailable));

            HttpResponseMessage response = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 1));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
        }

        [Test]
        public async Task GivenAnExpiredResultAndAFailingRefresh_WhenCheckingTheIpAgain_ThenTheFallbackIsServedAgain()
        {
            SetDetectionResponse(StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson));
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            int requestsBeforeExpiry = CountAnonymisationRequests();

            testHost.TimeProvider.Advance(TimeSpan.FromHours(6));
            SetDetectionResponse(StubHttpMessageHandler.FailingResponse(HttpStatusCode.ServiceUnavailable));
            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");

            HttpResponseMessage response = await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(CountAnonymisationRequests(), Is.EqualTo(requestsBeforeExpiry + 2));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
        }

        private void SetDetectionResponse(HttpResponseMessage response)
            => httpMessageHandler.SetResponder(
                StubHttpMessageHandler.Routing(
                    IsAnonymisationRequest,
                    IsGeolocationRequest,
                    _ => response,
                    _ => StubHttpMessageHandler.JsonResponse(AllowedCountryGeolocationJson),
                    _ => StubHttpMessageHandler.FailingResponse(HttpStatusCode.NotFound)));

        private int CountAnonymisationRequests()
            => httpMessageHandler.RequestedUrls.Count(url => IsAnonymisationRequest(url));
    }
}
