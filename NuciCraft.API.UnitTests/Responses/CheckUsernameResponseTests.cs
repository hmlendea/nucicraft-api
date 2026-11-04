using System.Linq;
using System.Text.Json;

using NuciAPI.Responses;

using NUnit.Framework;

using NuciCraft.API.Responses;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class CheckUsernameResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenACheckUsernameResponse_WhenSerialising_ThenTheUsernameCheckPropertiesAreContentProperties()
        {
            NuciApiContentResponse<CheckUsernameResponse> response = new(new CheckUsernameResponse
            {
                IsAllowed = false,
                DeniedReason = "Unallowed username. Use another one"
            });

            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(response, jsonSerializerOptions);
            JsonElement contentElement = responseDocument.RootElement.GetProperty("content");

            Assert.Multiple(() =>
            {
                Assert.That(
                    contentElement.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(["isAllowed", "deniedReason"]));
                Assert.That(
                    contentElement.GetProperty("isAllowed").GetBoolean(),
                    Is.False);
                Assert.That(
                    contentElement.GetProperty("deniedReason").GetString(),
                    Is.EqualTo("Unallowed username. Use another one"));
            });
        }
    }
}
