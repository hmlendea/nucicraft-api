using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetWorldsResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenNullWorlds_WhenGettingTheCount_ThenZeroIsReturned()
        {
            GetWorldsResponse response = new()
            {
                Worlds = null
            };

            Assert.That(response.Count, Is.Zero);
        }

        [Test]
        public void GivenTwoWorlds_WhenGettingTheCount_ThenTwoIsReturned()
        {
            IEnumerable<World> worlds =
            [
                new(),
                new(),
            ];
            GetWorldsResponse response = new()
            {
                Worlds = worlds
            };

            Assert.That(response.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenAGetWorldsResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
        {
            NuciApiContentResponse<GetWorldsResponse> response = new(new()
            {
                Worlds = [new World()]
            });
            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(
                response,
                jsonSerializerOptions);
            JsonElement contentElement = responseDocument.RootElement.GetProperty("content");

            Assert.That(
                responseDocument.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["code", "content", "hmac", "message", "success"]));
            Assert.That(
                contentElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["count", "worlds"]));
        }
    }
}
