using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetRtpLocationResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGetRtpLocationResponse_WhenSerialising_ThenTheLocationPropertiesAreContentProperties()
        {
            NuciApiContentResponse<GetRtpLocationResponse> response = new(new GetRtpLocationResponse(new()
            {
                Id = "solara",
                Biome = "Forest"
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
                Does.Contain("biome"));
            Assert.That(
                contentElement.TryGetProperty("rtpLocation", out JsonElement _),
                Is.False);
            Assert.That(
                contentElement.GetProperty("biome").GetString(),
                Is.EqualTo("Forest"));
        }
    }
}