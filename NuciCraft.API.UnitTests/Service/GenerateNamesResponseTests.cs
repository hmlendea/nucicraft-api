using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class GenerateNamesResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGenerateNamesResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
        {
            NuciApiContentResponse<GenerateNamesResponse> response = new(new()
            {
                Names = ["Solaire"]
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
                Is.EquivalentTo(["names"]));
        }
    }
}