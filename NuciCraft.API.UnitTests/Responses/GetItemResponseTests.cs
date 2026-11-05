using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetItemResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAnItem_WhenCreatingTheResponse_ThenTheItemPropertiesAreProjected()
        {
            Item item = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND",
                NuciCraftId = "nucicraft:diamond",
                SignIds = ["TestSignId"]
            };

            GetItemResponse response = new(item);

            Assert.That(response.Identifier, Is.EqualTo(item.Identifier));
            Assert.That(response.MinecraftId, Is.EqualTo(item.MinecraftId));
            Assert.That(response.BukkitId, Is.EqualTo(item.BukkitId));
            Assert.That(response.SignIds, Is.EqualTo(item.SignIds));
        }

        [Test]
        public void GivenAGetItemResponse_WhenSerialising_ThenTheIdentifierIsSerialisedAsId()
        {
            NuciApiContentResponse<GetItemResponse> response = new(new GetItemResponse(new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND",
                SignIds = ["TestSignId"]
            }));
            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(
                response,
                jsonSerializerOptions);
            JsonElement contentElement = responseDocument.RootElement.GetProperty("content");

            Assert.That(
                responseDocument.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["code", "content", "hmac", "message", "success"]));
            Assert.That(
                contentElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["id", "minecraftId", "bukkitId", "signIds"]));
            Assert.That(
                contentElement.GetProperty("id").GetString(),
                Is.EqualTo("test_item"));
            Assert.That(
                contentElement.GetProperty("minecraftId").GetString(),
                Is.EqualTo("minecraft:diamond"));
            Assert.That(
                contentElement.GetProperty("bukkitId").GetString(),
                Is.EqualTo("DIAMOND"));
            Assert.That(
                contentElement.GetProperty("signIds").EnumerateArray().Select(signId => signId.GetString()),
                Is.EqualTo(["TestSignId"]));
        }
    }
}
