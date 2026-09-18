using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetCountryResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGetCountryResponse_WhenSerialising_ThenTheCountryPropertiesAreContentProperties()
        {
            NuciApiContentResponse<GetCountryResponse> response = new(new GetCountryResponse(new()
            {
                Identifier = "nucilandia",
                Leader = "Testy McTestface"
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
                Does.Contain("leader"));
            Assert.That(
                contentElement.TryGetProperty("country", out JsonElement _),
                Is.False);
            Assert.That(
                contentElement.GetProperty("leader").GetString(),
                Is.EqualTo("Testy McTestface"));
        }
    }
}