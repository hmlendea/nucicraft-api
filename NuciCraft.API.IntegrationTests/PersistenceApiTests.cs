using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class PersistenceApiTests
    {
        private static string StoreDirectoryName => "nucicraft-api-persistence-tests";

        [Test]
        public async Task GivenAPatchedHome_WhenTheApplicationRestarts_ThenItsStateAndUniquenessPersist()
        {
            string storeDirectory = Path.Combine(Path.GetTempPath(), StoreDirectoryName, Guid.NewGuid().ToString("N"));
            string homeIdentifier;
            string homeJson;
            string playerIdentifier;

            try
            {
                using (ApiTestHost firstHost = new(storeDirectory))
                using (HttpClient firstClient = firstHost.CreateAuthorisedClient())
                {
                    using HttpResponseMessage playerResponse = await firstClient.PostAsync(
                        "/players", """{"username":"DummyUser"}""".CreateJsonContent());
                    Assert.That(playerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    using HttpResponseMessage getPlayerResponse = await firstClient.GetAsync(
                        "/players/by-username/DummyUser");
                    Assert.That(getPlayerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    using JsonDocument player = JsonDocument.Parse(
                        await getPlayerResponse.Content.ReadAsStringAsync());
                    playerIdentifier = player.RootElement
                        .GetProperty("content")
                        .GetProperty("id")
                        .GetString()!;
                    using HttpResponseMessage createResponse = await firstClient.PostAsync(
                        "/homes",
                        JsonSerializer.Serialize(new
                        {
                            Player = playerIdentifier,
                            Name = new { English = "Astora" },
                            Location = new { World = "world", X = 42, Y = 64, Z = 613 }
                        }).CreateJsonContent());
                    Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
                    homeIdentifier = created.RootElement.GetProperty("content").GetProperty("id").GetString()!;
                    using HttpResponseMessage patchResponse = await firstClient.PatchAsync(
                        $"/homes/{homeIdentifier}", """{"name":{"romanian":"Anor Londo"}}""".CreateJsonContent());
                    Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    using JsonDocument patched = JsonDocument.Parse(await patchResponse.Content.ReadAsStringAsync());
                    homeJson = patched.RootElement.GetProperty("content").GetRawText();
                }

                using ApiTestHost secondHost = new(storeDirectory);
                using HttpClient secondClient = secondHost.CreateAuthorisedClient();
                using HttpResponseMessage getResponse = await secondClient.GetAsync($"/homes/{homeIdentifier}");
                using JsonDocument retrieved = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());

                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(retrieved.RootElement.GetProperty("content").GetRawText(), Is.EqualTo(homeJson));
                using HttpResponseMessage duplicateResponse = await secondClient.PostAsync(
                    "/homes",
                    JsonSerializer.Serialize(new
                    {
                        Player = playerIdentifier,
                        Name = new { Default = "anor londo" },
                        Location = new { World = "world" }
                    }).CreateJsonContent());

                Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }
            finally
            {
                if (Directory.Exists(storeDirectory))
                {
                    Directory.Delete(storeDirectory, true);
                }
            }
        }

        [Test]
        public async Task GivenAWorld_WhenTheApplicationRestarts_ThenTheWorldPersists()
        {
            string storeDirectory = Path.Combine(
                Path.GetTempPath(),
                StoreDirectoryName,
                Guid.NewGuid().ToString("N"));

            try
            {
                using (ApiTestHost firstHost = new(storeDirectory))
                using (HttpClient firstClient = firstHost.CreateAuthorisedClient())
                {
                    HttpResponseMessage createResponse = await firstClient.PostAsync(
                        "/worlds",
                        "{\"id\":\"overworld\",\"hasWebMap\":true}".CreateJsonContent());

                    Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                }

                using ApiTestHost secondHost = new(storeDirectory);
                using HttpClient secondClient = secondHost.CreateAuthorisedClient();
                HttpResponseMessage getResponse = await secondClient.GetAsync("/worlds/overworld");

                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
            finally
            {
                if (Directory.Exists(storeDirectory))
                {
                    Directory.Delete(storeDirectory, true);
                }
            }
        }
    }
}