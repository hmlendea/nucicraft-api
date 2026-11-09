using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciDAL.Repositories;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class AccessControlIpUsernameApiTests
    {
        private static string AllowedCountryGeolocationJson
            => """{"status":"success","countryCode":"US","country":"United States"}""";
        private static string DeniedCountryGeolocationJson
            => """{"status":"success","countryCode":"CN","country":"China"}""";
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
        public async Task GivenAnUnregisteredUsername_WhenCheckingADeniedIp_ThenTheIpIsDenied()
        {
            UseDeniedCountryGeolocation();

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted from this country.\""));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingADeniedIp_ThenTheIpIsAllowed()
        {
            UseDeniedCountryGeolocation();
            RegisterPlayer("DummyUser", false);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsername_WhenCheckingTheIp_ThenNoExternalCheckIsPerformed()
        {
            RegisterPlayer("DummyUser", false);

            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");

            Assert.That(httpMessageHandler.RequestCount, Is.Zero);
        }

        [Test]
        public async Task GivenARegisteredBannedUsername_WhenCheckingADeniedIp_ThenTheIpIsDenied()
        {
            UseDeniedCountryGeolocation();
            RegisterPlayer("GrumpyCat", true);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=GrumpyCat");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"This IP address is associated with a user banned on NuciCraft\""));
        }

        [Test]
        public async Task GivenNoUsername_WhenCheckingADeniedIp_ThenTheIpIsDenied()
        {
            UseDeniedCountryGeolocation();
            RegisterPlayer("DummyUser", false);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameWithDifferentCasing_WhenCheckingADeniedIp_ThenTheIpIsDenied()
        {
            UseDeniedCountryGeolocation();
            RegisterPlayer("DummyUser", false);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=dummyuser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
        }

        [Test]
        public async Task GivenADisabledRegisteredPlayerExemption_WhenCheckingADeniedIpForARegisteredPlayer_ThenTheIpIsDenied()
        {
            using AccessControlApiTestHost disabledTestHost = new(httpMessageHandler, false);
            using HttpClient disabledClient = disabledTestHost.CreateAuthorisedClient();

            disabledTestHost.PlayerRepository.Add(BuildPlayer("DummyUser", false));
            UseDeniedCountryGeolocation();

            HttpResponseMessage response = await disabledClient.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted from this country.\""));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingItWithoutAUsername_ThenTheIpAddressIsDenied()
        {
            testHost.PlayerRepository.Add(BuildBannedPlayer("GrumpyCat", "8.8.8.8"));

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"This IP address is banned on NuciCraft\""));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingItWithAUsername_ThenTheUserIsDenied()
        {
            testHost.PlayerRepository.Add(BuildBannedPlayer("GrumpyCat", "8.8.8.8"));

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"This IP address is associated with a user banned on NuciCraft\""));
        }

        [Test]
        public async Task GivenAnIpAddressOfABannedPlayer_WhenCheckingIt_ThenNoExternalCheckIsPerformed()
        {
            testHost.PlayerRepository.Add(BuildBannedPlayer("GrumpyCat", "8.8.8.8"));

            await client.GetAsync("/accesscontrol/check-ip?ipAddress=8.8.8.8");

            Assert.That(httpMessageHandler.RequestCount, Is.Zero);
        }

        [Test]
        public async Task GivenARegisteredNonBannedUsernameAndAnIpAddressOfABannedPlayer_WhenCheckingTheIp_ThenTheIpIsAllowed()
        {
            testHost.PlayerRepository.Add(BuildBannedPlayer("GrumpyCat", "8.8.8.8"));
            RegisterPlayer("DummyUser", false);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8&username=DummyUser");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenAnIpAddressOfANonBannedPlayer_WhenCheckingIt_ThenTheIpIsAllowed()
        {
            PlayerDataObject player = BuildPlayer("IlarionPintilie", false);
            player.LastIpAddress = "8.8.8.8";

            testHost.PlayerRepository.Add(player);

            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
        }

        private void RegisterPlayer(string username, bool isBanned)
            => testHost.PlayerRepository.Add(BuildPlayer(username, isBanned));

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

        private void UseDeniedCountryGeolocation()
            => httpMessageHandler.SetResponder(
                StubHttpMessageHandler.Routing(
                    IsAnonymisationRequest,
                    IsGeolocationRequest,
                    _ => StubHttpMessageHandler.JsonResponse(AnonymisationServiceNotDetectedJson),
                    _ => StubHttpMessageHandler.JsonResponse(DeniedCountryGeolocationJson),
                    _ => StubHttpMessageHandler.FailingResponse(HttpStatusCode.NotFound)));
    }
}
