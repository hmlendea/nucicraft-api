using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using NUnit.Framework;

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
            GetWorldsResponse response = new()
            {
                Worlds = [new World()]
            };
            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(
                response,
                jsonSerializerOptions);

            Assert.That(
                responseDocument.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["code", "count", "hmac", "message", "success", "worlds"]));
        }
    }
}
