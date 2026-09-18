using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class PlayersApiTests
    {
        private ApiTestHost testHost = null!;
        private HttpClient client = null!;

        [SetUp]
        public void SetUp()
        {
            testHost = new ApiTestHost();
            client = testHost.CreateAuthorisedClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            testHost.Dispose();
        }

        [Test]
        public async Task GivenAPlayer_WhenRegisteringAndUsingEveryLookupAndPatchSelector_ThenAllRoutesSucceed()
        {
            HttpResponseMessage registerResponse = await client.PostAsync(
                "/players",
                "{\"username\":\"Angetenar\",\"onlineUUID\":\"online-uuid\"}".CreateJsonContent());
            HttpResponseMessage listResponse = await client.GetAsync("/players");
            string listBody = await listResponse.Content.ReadAsStringAsync();
            using JsonDocument listDocument = JsonDocument.Parse(listBody);
            JsonElement playerElement = listDocument.RootElement
                .GetProperty("content")
                .GetProperty("players")[0];
            string identifier = playerElement.GetProperty("id").GetString()!;
            string offlineUuid = playerElement.GetProperty("offlineUUID").GetString()!;

            HttpResponseMessage getByIdentifierResponse = await client.GetAsync($"/players/{identifier}");
            HttpResponseMessage getByUsernameResponse = await client.GetAsync("/players/by-username/Angetenar");
            HttpResponseMessage getByOfflineUuidResponse = await client.GetAsync($"/players/by-offline-uuid/{offlineUuid}");
            HttpResponseMessage getByOnlineUuidResponse = await client.GetAsync("/players/by-online-uuid/online-uuid");
            HttpResponseMessage patchByIdentifierResponse = await client.PatchAsync(
                $"/players/{identifier}",
                "{\"displayName\":\"Ilarion Pintilie\"}".CreateJsonContent());
            HttpResponseMessage patchByUsernameResponse = await client.PatchAsync(
                "/players/by-username/Angetenar",
                "{}".CreateJsonContent());
            HttpResponseMessage patchByOfflineUuidResponse = await client.PatchAsync(
                $"/players/by-offline-uuid/{offlineUuid}",
                "{}".CreateJsonContent());
            HttpResponseMessage patchByOnlineUuidResponse = await client.PatchAsync(
                "/players/by-online-uuid/online-uuid",
                "{}".CreateJsonContent());

            Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByIdentifierResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByUsernameResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByOfflineUuidResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByOnlineUuidResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchByIdentifierResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchByUsernameResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchByOfflineUuidResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchByOnlineUuidResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}