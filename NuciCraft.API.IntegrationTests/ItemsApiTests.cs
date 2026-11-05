using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ItemsApiTests
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
        public async Task GivenAnItem_WhenCreatingAndReadingItByEverySelector_ThenTheItemIsReturned()
        {
            HttpResponseMessage createResponse = await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"nucicraftId\":\"nucicraft:diamond\",\"bukkitId\":\"DIAMOND\"}"
                    .CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync("/items/test_item");
            HttpResponseMessage getByMinecraftIdResponse = await client.GetAsync("/items/by-minecraft-id/minecraft:diamond");
            HttpResponseMessage getByBukkitIdResponse = await client.GetAsync("/items/by-bukkit-id/DIAMOND");
            HttpResponseMessage getByNuciCraftIdResponse = await client.GetAsync("/items/by-nucicraft-id/nucicraft:diamond");

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByMinecraftIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByBukkitIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByNuciCraftIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            JsonDocument getByMinecraftIdDocument = JsonDocument.Parse(
                await getByMinecraftIdResponse.Content.ReadAsStringAsync());
            JsonElement getByMinecraftIdContent = getByMinecraftIdDocument.RootElement.GetProperty("content");
            JsonDocument getByBukkitIdDocument = JsonDocument.Parse(
                await getByBukkitIdResponse.Content.ReadAsStringAsync());
            JsonElement getByBukkitIdContent = getByBukkitIdDocument.RootElement.GetProperty("content");
            JsonDocument getByNuciCraftIdDocument = JsonDocument.Parse(
                await getByNuciCraftIdResponse.Content.ReadAsStringAsync());
            JsonElement getByNuciCraftIdContent = getByNuciCraftIdDocument.RootElement.GetProperty("content");

            Assert.That(getByMinecraftIdContent.GetProperty("id").GetString(), Is.EqualTo("test_item"));
            Assert.That(getByBukkitIdContent.GetProperty("id").GetString(), Is.EqualTo("test_item"));
            Assert.That(getByNuciCraftIdContent.GetProperty("id").GetString(), Is.EqualTo("test_item"));
        }

        [Test]
        public async Task GivenASavedItem_WhenListingItems_ThenTheItemIsListed()
        {
            await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"bukkitId\":\"DIAMOND\"}"
                    .CreateJsonContent());

            HttpResponseMessage listResponse = await client.GetAsync("/items");

            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            JsonDocument listResponseDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
            JsonElement listedItem = listResponseDocument.RootElement
                .GetProperty("content")
                .GetProperty("items")
                .EnumerateArray()
                .Single(item => item.GetProperty("minecraftId").GetString() == "minecraft:diamond");

            Assert.That(listedItem.GetProperty("minecraftId").GetString(), Is.EqualTo("minecraft:diamond"));
            Assert.That(
                listResponseDocument.RootElement.GetProperty("content").GetProperty("count").GetInt32(),
                Is.EqualTo(1));
        }

        [Test]
        public async Task GivenASavedItem_WhenPatchingBukkitIdAndSignIds_ThenTheChangesPersist()
        {
            await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"bukkitId\":\"DIAMOND\"}"
                    .CreateJsonContent());

            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/items/test_item",
                "{\"id\":\"ignored\",\"bukkitId\":\"EMERALD\",\"signIds\":[\"Sign1\",\"Sign2\"]}".CreateJsonContent());
            HttpResponseMessage patchedGetResponse = await client.GetAsync("/items/test_item");

            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchedGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            JsonDocument patchedGetDocument = JsonDocument.Parse(
                await patchedGetResponse.Content.ReadAsStringAsync());
            JsonElement patchedContent = patchedGetDocument.RootElement.GetProperty("content");

            Assert.That(patchedContent.GetProperty("bukkitId").GetString(), Is.EqualTo("EMERALD"));
            Assert.That(
                patchedContent.GetProperty("signIds").EnumerateArray().Select(signId => signId.GetString()),
                Is.EqualTo(["Sign1", "Sign2"]));
        }

        [Test]
        public async Task GivenASavedItem_WhenAddingItAgain_ThenAConflictIsReturned()
        {
            await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"bukkitId\":\"DIAMOND\"}"
                    .CreateJsonContent());

            HttpResponseMessage duplicateResponse = await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:emerald\",\"bukkitId\":\"EMERALD\"}"
                    .CreateJsonContent());

            Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task GivenAnAbsentItem_WhenReadingItBySelector_ThenNotFoundIsReturned()
        {
            HttpResponseMessage getResponse = await client.GetAsync("/items/nonexistent");
            HttpResponseMessage getByMinecraftIdResponse = await client.GetAsync("/items/by-minecraft-id/minecraft:nonexistent");

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(getByMinecraftIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GivenAMissingMinecraftId_WhenAddingAnItem_ThenBadRequestIsReturned()
        {
            HttpResponseMessage response = await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"\",\"bukkitId\":\"DIAMOND\"}".CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GivenAMissingBukkitId_WhenAddingAnItem_ThenBadRequestIsReturned()
        {
            HttpResponseMessage response = await client.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"bukkitId\":\"\"}".CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GivenARequest_WithoutAuthorisation_WhenAddingAnItem_ThenUnauthorizedIsReturned()
        {
            HttpClient unauthorisedClient = testHost.CreateClient();
            unauthorisedClient.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

            HttpResponseMessage response = await unauthorisedClient.PostAsync(
                "/items",
                "{\"id\":\"test_item\",\"minecraftId\":\"minecraft:diamond\",\"bukkitId\":\"DIAMOND\"}"
                    .CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
