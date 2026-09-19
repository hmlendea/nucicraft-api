using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class HomesApiTests
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
        public async Task GivenARegisteredPlayer_WhenCreatingAndRetrievingAHome_ThenAllSelectorsReturnIt()
        {
            string playerIdentifier = await RegisterPlayerAsync("DummyUser");
            JsonElement created = await AddHomeAsync("DummyUser", "Astora");
            string homeIdentifier = created.GetProperty("id").GetString()!;

            Assert.That(Guid.Parse(homeIdentifier), Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.GetProperty("player").GetString(), Is.EqualTo(playerIdentifier));
            Assert.That(created.GetProperty("createdDT").GetDateTimeOffset(), Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));
            Assert.That(created.GetProperty("updatedDT").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(created.GetProperty("location").GetProperty("world").GetString(), Is.EqualTo("world"));

            foreach (string route in new[] { $"/homes/{homeIdentifier}", $"/homes?player={playerIdentifier}&name=astora" })
            {
                using HttpResponseMessage response = await client.GetAsync(route);
                JsonElement content = await ReadContentAsync(response);

                Assert.That(content.GetProperty("id").GetString(), Is.EqualTo(homeIdentifier));
            }

            foreach (string route in new[] { "/homes", $"/homes/by-player/{playerIdentifier}", $"/homes?player={playerIdentifier}" })
            {
                using HttpResponseMessage response = await client.GetAsync(route);
                JsonElement content = await ReadContentAsync(response);

                Assert.That(content.GetProperty("count").GetInt32(), Is.EqualTo(1));
                Assert.That(content.GetProperty("homes")[0].GetProperty("id").GetString(), Is.EqualTo(homeIdentifier));
            }
        }

        [Test]
        public async Task GivenAnExistingHome_WhenPatchingIt_ThenGeneratedFieldsAreProtected()
        {
            await RegisterPlayerAsync("DummyUser");
            JsonElement created = await AddHomeAsync("DummyUser", "Astora");
            string homeIdentifier = created.GetProperty("id").GetString()!;
            using HttpResponseMessage response = await client.PatchAsync(
                $"/homes/{homeIdentifier}",
                """
                {"id":"forged-id","createdDT":"2000-01-01T00:00:00Z","updatedDT":"2000-01-01T00:00:00Z",
                 "name":{"romanian":"Anor Londo"},"location":{"world":"world","x":613,"y":64,"z":873}}
                """.CreateJsonContent());
            JsonElement patched = await ReadContentAsync(response);

            Assert.That(patched.GetProperty("id").GetString(), Is.EqualTo(homeIdentifier));
            Assert.That(patched.GetProperty("createdDT").GetString(), Is.EqualTo(created.GetProperty("createdDT").GetString()));
            Assert.That(patched.GetProperty("updatedDT").GetDateTimeOffset(), Is.GreaterThanOrEqualTo(created.GetProperty("createdDT").GetDateTimeOffset()));
            Assert.That(patched.GetProperty("name").GetProperty("english").GetString(), Is.EqualTo("Astora"));
            Assert.That(patched.GetProperty("name").GetProperty("romanian").GetString(), Is.EqualTo("Anor Londo"));
            Assert.That(patched.GetProperty("location").GetProperty("x").GetSingle(), Is.EqualTo(613));
        }

        [Test]
        public async Task GivenTwoPlayers_WhenCreatingHomes_ThenNamesAreUniqueOnlyWithinEachPlayer()
        {
            string firstPlayer = await RegisterPlayerAsync("DummyUser");
            string secondPlayer = await RegisterPlayerAsync("Angetenar");
            JsonElement firstHome = await AddHomeAsync("DummyUser", "Astora");
            await AddHomeAsync("DummyUser", "Anor Londo");
            JsonElement secondHome = await AddHomeAsync("Angetenar", "Astora");
            using HttpResponseMessage duplicateResponse = await client.PostAsync(
                "/homes", BuildHomeJson("DummyUser", " ASTORA ").CreateJsonContent());

            Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            using HttpResponseMessage firstListResponse = await client.GetAsync($"/homes/by-player/{firstPlayer}");
            using HttpResponseMessage secondListResponse = await client.GetAsync($"/homes/by-player/{secondPlayer}");
            JsonElement firstList = await ReadContentAsync(firstListResponse);
            JsonElement secondList = await ReadContentAsync(secondListResponse);

            Assert.That(firstList.GetProperty("count").GetInt32(), Is.EqualTo(2));
            Assert.That(secondList.GetProperty("count").GetInt32(), Is.EqualTo(1));
            Assert.That(firstHome.GetProperty("id").GetString(), Is.Not.EqualTo(secondHome.GetProperty("id").GetString()));
        }

        [Test]
        public async Task GivenConflictingHomes_WhenRenamingOrTransferringOne_ThenTheOriginalRemainsIntact()
        {
            await RegisterPlayerAsync("DummyUser");
            string secondPlayer = await RegisterPlayerAsync("Angetenar");
            await AddHomeAsync("DummyUser", "Astora");
            JsonElement home = await AddHomeAsync("DummyUser", "Anor Londo");
            await AddHomeAsync("Angetenar", "Anor Londo");
            string homeIdentifier = home.GetProperty("id").GetString()!;
            using HttpResponseMessage renameResponse = await client.PatchAsync(
                $"/homes/{homeIdentifier}",
                """{"name":{"romanian":"astora"}}""".CreateJsonContent());
            using HttpResponseMessage transferResponse = await client.PatchAsync(
                $"/homes/{homeIdentifier}",
                JsonSerializer.Serialize(new { Player = secondPlayer }).CreateJsonContent());

            Assert.That(renameResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(transferResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            using HttpResponseMessage getResponse = await client.GetAsync($"/homes/{homeIdentifier}");
            JsonElement unchanged = await ReadContentAsync(getResponse);

            Assert.That(unchanged.GetRawText(), Is.EqualTo(home.GetRawText()));
        }

        [Test]
        public async Task GivenAnAvailableNameForAnotherPlayer_WhenTransferringAHome_ThenTheIdentifierIsUsed()
        {
            string originalPlayer = await RegisterPlayerAsync("DummyUser");
            string destinationPlayer = await RegisterPlayerAsync("Angetenar");
            JsonElement home = await AddHomeAsync("DummyUser", "Astora");
            string identifier = home.GetProperty("id").GetString()!;
            using HttpResponseMessage patchResponse = await client.PatchAsync(
                $"/homes/{identifier}",
                JsonSerializer.Serialize(new { Player = destinationPlayer }).CreateJsonContent());
            JsonElement transferred = await ReadContentAsync(patchResponse);

            Assert.That(transferred.GetProperty("player").GetString(), Is.EqualTo(destinationPlayer));
            using HttpResponseMessage originalResponse = await client.GetAsync($"/homes?player={originalPlayer}");
            JsonElement originalHomes = await ReadContentAsync(originalResponse);

            Assert.That(originalHomes.GetProperty("count").GetInt32(), Is.Zero);
            using HttpResponseMessage lookupResponse = await client.GetAsync(
                $"/homes?player={destinationPlayer}&name=Astora");
            JsonElement retrieved = await ReadContentAsync(lookupResponse);

            Assert.That(retrieved.GetProperty("id").GetString(), Is.EqualTo(identifier));
        }

        [Test]
        public async Task GivenClientSuppliedMetadata_WhenCreatingAHome_ThenTheApiGeneratesItsOwnValues()
        {
            await RegisterPlayerAsync("DummyUser");
            DateTimeOffset earliestCreation = DateTimeOffset.UtcNow;
            using HttpResponseMessage response = await client.PostAsync(
                "/homes",
                """
                {"id":"forged-id","createdDT":"2000-01-01T00:00:00Z","updatedDT":"2000-01-01T00:00:00Z",
                 "player":"DummyUser","name":{"default":"Astora"},"location":{"world":"world","x":42,"y":64,"z":613}}
                """.CreateJsonContent());
            JsonElement home = await ReadContentAsync(response);

            Assert.That(Guid.Parse(home.GetProperty("id").GetString()!), Is.Not.EqualTo(Guid.Empty));
            Assert.That(home.GetProperty("createdDT").GetDateTimeOffset(), Is.InRange(earliestCreation, DateTimeOffset.UtcNow));
            Assert.That(home.GetProperty("updatedDT").ValueKind, Is.EqualTo(JsonValueKind.Null));
        }

        [TestCase("{}")]
        [TestCase("{\"player\":\"DummyUser\",\"location\":{\"world\":\"world\"}}")]
        [TestCase("{\"player\":\"DummyUser\",\"name\":{\"english\":\"Astora\"}}")]
        [TestCase("{\"name\":{\"english\":\"Astora\"},\"location\":{\"world\":\"world\"}}")]
        [TestCase("{\"player\":\"DummyUser\",\"name\":{},\"location\":{\"world\":\"world\"}}")]
        [TestCase("{\"player\":\"DummyUser\",\"name\":{\"english\":\" \"},\"location\":{\"world\":\"world\"}}")]
        [TestCase("{\"player\":\"DummyUser\",\"name\":{\"english\":\"Astora\"},\"location\":{}}")]
        [TestCase("{\"player\":\"DummyUser\",\"name\":\"Astora\",\"location\":{\"world\":\"world\"}}")]
        [TestCase("{")]
        public async Task GivenAnInvalidCreationRequest_WhenCreatingAHome_ThenBadRequestIsReturned(string body)
        {
            await RegisterPlayerAsync("DummyUser");
            using HttpResponseMessage response = await client.PostAsync("/homes", body.CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase("/homes?name=Astora")]
        [TestCase("/homes?name=")]
        [TestCase("/homes?player=")]
        [TestCase("/homes?player=player-id&name=")]
        public async Task GivenIncompleteQuerySelectors_WhenRetrievingHomes_ThenBadRequestIsReturned(string route)
        {
            using HttpResponseMessage response = await client.GetAsync(route);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase("/homes/missing-home-id")]
        [TestCase("/homes?player=missing-player-id&name=Astora")]
        public async Task GivenAnAbsentHome_WhenRetrievingIt_ThenNotFoundIsReturned(string route)
        {
            using HttpResponseMessage response = await client.GetAsync(route);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TestCase("/homes")]
        [TestCase("/homes/by-player/missing-player-id")]
        [TestCase("/homes?player=missing-player-id")]
        public async Task GivenNoHomes_WhenListingHomes_ThenAnEmptyCollectionIsReturned(string route)
        {
            using HttpResponseMessage response = await client.GetAsync(route);
            JsonElement content = await ReadContentAsync(response);

            Assert.That(content.GetProperty("count").GetInt32(), Is.Zero);
            Assert.That(content.GetProperty("homes").EnumerateArray(), Is.Empty);
        }

        [Test]
        public async Task GivenAnUnregisteredUsername_WhenCreatingAHome_ThenNotFoundIsReturned()
        {
            using HttpResponseMessage response = await client.PostAsync(
                "/homes", BuildHomeJson("DummyUser", "Astora").CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GivenAPlayerIdentifierInsteadOfAUsername_WhenCreatingAHome_ThenItIsNotResolvedAsAUsername()
        {
            string playerIdentifier = await RegisterPlayerAsync("DummyUser");
            using HttpResponseMessage response = await client.PostAsync(
                "/homes", BuildHomeJson(playerIdentifier, "Astora").CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TestCase(null)]
        [TestCase("DummyUser")]
        public async Task GivenMissingOrInvalidAuthorisation_WhenUsingHomesRoutes_ThenAccessIsDenied(string? apiKey)
        {
            client.DefaultRequestHeaders.Authorization = null;

            if (apiKey is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            foreach (string route in new[] { "/homes", "/homes/home-id", "/homes/by-player/player-id", "/homes?player=player-id&name=Astora" })
            {
                using HttpResponseMessage response = await client.GetAsync(route);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }

            using HttpResponseMessage createResponse = await client.PostAsync(
                "/homes", BuildHomeJson("DummyUser", "Astora").CreateJsonContent());
            using HttpResponseMessage patchResponse = await client.PatchAsync(
                "/homes/home-id", "{}".CreateJsonContent());

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        private async Task<string> RegisterPlayerAsync(string username)
        {
            using HttpResponseMessage registration = await client.PostAsync(
                "/players",
                JsonSerializer.Serialize(new { Username = username }).CreateJsonContent());
            Assert.That(registration.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using HttpResponseMessage response = await client.GetAsync($"/players/by-username/{username}");
            JsonElement player = await ReadContentAsync(response);

            return player.GetProperty("id").GetString()!;
        }

        private async Task<JsonElement> AddHomeAsync(string username, string name)
        {
            using HttpResponseMessage response = await client.PostAsync(
                "/homes",
                BuildHomeJson(username, name).CreateJsonContent());

            return await ReadContentAsync(response);
        }

        private static async Task<JsonElement> ReadContentAsync(HttpResponseMessage response)
        {
            string body = await response.Content.ReadAsStringAsync();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
            using JsonDocument document = JsonDocument.Parse(body);

            return document.RootElement.GetProperty("content").Clone();
        }

        private static string BuildHomeJson(string username, string name) => JsonSerializer.Serialize(new
        {
            Player = username,
            Name = new { English = name },
            Location = new { World = "world", X = 42, Y = 64, Z = 613 }
        });
    }
}