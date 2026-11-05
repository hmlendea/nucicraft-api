using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetMobNameResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGetMobNameResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
        {
            NuciApiContentResponse<GetMobNameResponse> response = new(new()
            {
                Names = ["Ilarion"]
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
                Is.EqualTo(["names"]));
            Assert.That(
                contentElement.GetProperty("names").EnumerateArray().Select(name => name.GetString()),
                Is.EqualTo(["Ilarion"]));
        }
    }
}
