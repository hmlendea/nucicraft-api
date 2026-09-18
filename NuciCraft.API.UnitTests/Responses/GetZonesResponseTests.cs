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
    public sealed class GetZonesResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenNullZones_WhenGettingTheCount_ThenZeroIsReturned()
        {
            GetZonesResponse response = new()
            {
                Zones = null
            };

            Assert.That(response.Count, Is.Zero);
        }

        [Test]
        public void GivenTwoZones_WhenGettingTheCount_ThenTwoIsReturned()
        {
            IEnumerable<Zone> zones =
            [
                new(),
                new(),
            ];
            GetZonesResponse response = new()
            {
                Zones = zones
            };

            Assert.That(response.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenAGetZonesResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
        {
            NuciApiContentResponse<GetZonesResponse> response = new(new()
            {
                Zones = [new Zone()]
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
                Is.EquivalentTo(["count", "zones"]));
        }
    }
}