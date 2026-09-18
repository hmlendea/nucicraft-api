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
        public void GivenAGenerateNamesResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
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
                Is.EquivalentTo(["code", "hmac", "message", "names", "success"]));
        }
    }
}