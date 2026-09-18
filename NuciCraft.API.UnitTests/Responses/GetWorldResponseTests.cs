using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetWorldResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGetWorldResponse_WhenSerialising_ThenTheWorldPropertiesAreContentProperties()
        {
            NuciApiContentResponse<GetWorldResponse> response = new(new GetWorldResponse(new()
            {
                Identifier = "world",
                HasWebMap = true
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
                Does.Not.Contain("identifier"));
            Assert.That(
                contentElement.TryGetProperty("world", out JsonElement _),
                Is.False);
            Assert.That(
                contentElement.GetProperty("id").GetString(),
                Is.EqualTo("world"));
        }
    }
}
