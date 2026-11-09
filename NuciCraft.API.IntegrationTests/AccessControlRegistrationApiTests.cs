using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class AccessControlRegistrationApiTests
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
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingTheIp_ThenTheIpIsAllowedWithoutAnAnonymisationCheck()
        {
            testHost.PlayerRepository.Add(BuildPlayer(RegisteredUsername, false));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-ip?ipAddress=8.8.8.8&username={RegisteredUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
            Assert.That(httpMessageHandler.RequestCount, Is.Zero);
        }

        [Test]
        public async Task GivenARegisteredBannedUsername_WhenCheckingTheIp_ThenTheIpIsDeniedWithoutAnAnonymisationCheck()
        {
            testHost.PlayerRepository.Add(BuildPlayer(BannedUsername, true));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-ip?ipAddress=8.8.8.8&username={BannedUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("associated with a banned user"));
            Assert.That(httpMessageHandler.RequestCount, Is.Zero);
        }

        [Test]
        public async Task GivenAnUnregisteredUsername_WhenCheckingTheIp_ThenTheAnonymisationCheckIsPerformed()
        {
            UseDetectedAnonymisationService();

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-ip?ipAddress=8.8.8.8&username={UnregisteredUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
            Assert.That(CountAnonymisationRequests(), Is.EqualTo(1));
        }

        [TestCase("")]
        [TestCase("%20%20%20")]
        public async Task GivenAnAbsentUsername_WhenCheckingTheIp_ThenTheAnonymisationCheckIsPerformed(
            string usernameQuery)
        {
            UseDetectedAnonymisationService();

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-ip?ipAddress=8.8.8.8&username={usernameQuery}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted via any anonymisation service.\""));
            Assert.That(CountAnonymisationRequests(), Is.EqualTo(1));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsAllowed()
        {
            testHost.PlayerRepository.Add(BuildPlayer(BannedUsername, false));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-username?username={BannedUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenARegisteredBannedUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDeniedWithTheBanReason()
        {
            testHost.PlayerRepository.Add(BuildPlayer(BannedUsername, true));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-username?username={BannedUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("banned"));
            Assert.That(body, Does.Contain(BanReason));
        }

        [Test]
        public async Task GivenAnUnregisteredUsernameMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDenied()
        {
            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-username?username={BannedUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"This username is not permitted.\""));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsAllowed()
        {
            testHost.PlayerRepository.Add(BuildPlayer(RegisteredUsername, false));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-username?username={RegisteredUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenARegisteredBannedUsernameNotMatchingADeniedPattern_WhenCheckingTheUsername_ThenTheUsernameIsDeniedWithTheBanReason()
        {
            testHost.PlayerRepository.Add(BuildPlayer(RegisteredUsername, true));

            HttpResponseMessage response = await client.GetAsync(
                $"/accesscontrol/check-username?username={RegisteredUsername}");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("banned"));
            Assert.That(body, Does.Contain(BanReason));
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

        private void UseDetectedAnonymisationService()
            => httpMessageHandler.SetResponder(
                StubHttpMessageHandler.Routing(
                    IsAnonymisationRequest,
                    IsGeolocationRequest,
                    _ => StubHttpMessageHandler.JsonResponse(AnonymisationServiceDetectedJson),
                    _ => StubHttpMessageHandler.JsonResponse(AllowedCountryGeolocationJson),
                    _ => StubHttpMessageHandler.FailingResponse(HttpStatusCode.NotFound)));

        private int CountAnonymisationRequests()
            => httpMessageHandler.RequestedUrls.Count(url => IsAnonymisationRequest(url));
    }
}
