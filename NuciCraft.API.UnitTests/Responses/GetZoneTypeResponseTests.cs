using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetZoneTypeResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGetZoneTypeResponse_WhenSerialising_ThenTheZoneTypePropertiesAreContentProperties()
        {
            NuciApiContentResponse<GetZoneTypeResponse> response = new(new GetZoneTypeResponse(new()
            {
                Categories = ["settlement", "civilian"],
                Identifier = "city"
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
                Does.Contain("id"));
            Assert.That(
                contentElement.EnumerateObject().Select(property => property.Name),
                Does.Contain("categories"));
            Assert.That(
                contentElement.EnumerateObject().Select(property => property.Name),
                Does.Not.Contain("identifier"));
            Assert.That(
                contentElement.TryGetProperty("zoneType", out JsonElement _),
                Is.False);
            Assert.That(
                contentElement.GetProperty("id").GetString(),
                Is.EqualTo("city"));
            Assert.That(
                contentElement.GetProperty("categories").EnumerateArray().Select(category => category.GetString()),
                Is.EqualTo(new[] { "settlement", "civilian" }));
        }
    }
}