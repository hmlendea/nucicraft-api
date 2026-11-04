using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class CheckIpResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenACheckIpResponse_WhenSerialising_ThenTheIpCheckPropertiesAreContentProperties()
        {
            NuciApiContentResponse<CheckIpResponse> response = new(new CheckIpResponse
            {
                IsAllowed = true,
                CountryCode = "RO"
            });

            using JsonDocument responseDocument = JsonSerializer.SerializeToDocument(response, jsonSerializerOptions);
            JsonElement contentElement = responseDocument.RootElement.GetProperty("content");

            Assert.Multiple(() =>
            {
                Assert.That(
                    contentElement.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(["isAllowed", "deniedReason", "countryCode"]));
                Assert.That(
                    contentElement.GetProperty("isAllowed").GetBoolean(),
                    Is.True);
                Assert.That(
                    contentElement.GetProperty("countryCode").GetString(),
                    Is.EqualTo("RO"));
            });
        }
    }
}
