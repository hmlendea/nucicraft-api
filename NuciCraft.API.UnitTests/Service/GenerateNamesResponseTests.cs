using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class GenerateNamesResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAGenerateNamesResponse_WhenSerialising_ThenTheLatestUngRootContractIsPreserved()
        {
            GenerateNamesResponse response = new()
            {
                Names = ["Solaire"]
            };
            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(
                response,
                jsonSerializerOptions);

            Assert.That(
                responseDocument.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(["code", "content", "hmac", "message", "names", "success"]));
            Assert.That(
                responseDocument.RootElement.GetProperty("content").ValueKind,
                Is.EqualTo(JsonValueKind.Null));
            Assert.That(
                responseDocument.RootElement
                    .GetProperty("names")
                    .EnumerateArray()
                    .Select(element => element.GetString()),
                Is.EqualTo(["Solaire"]));
        }
    }
}
