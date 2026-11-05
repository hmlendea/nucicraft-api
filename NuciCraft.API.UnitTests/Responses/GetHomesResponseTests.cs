using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetHomesResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [TestCase(0)]
        [TestCase(2)]
        public void GivenHomes_WhenGettingTheCount_ThenTheHomeCountIsReturned(int homeCount)
        {
            List<Home> homes = [.. Enumerable.Repeat(new Home(), homeCount)];
            GetHomesResponse response = new()
            {
                Homes = homes
            };

            Assert.That(response.Count, Is.EqualTo(homeCount));
        }

        [Test]
        public void GivenAnEmptyHomesList_WhenGettingTheCount_ThenZeroIsReturned()
        {
            GetHomesResponse response = new()
            {
                Homes = []
            };

            Assert.That(response.Count, Is.Zero);
        }

        [Test]
        public void GivenAGetHomesResponse_WhenSerialising_ThenTheCurrentRootContractIsPreserved()
        {
            NuciApiContentResponse<GetHomesResponse> response = new(new()
            {
                Homes = [new Home()]
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
                Is.EquivalentTo(["homes", "count"]));
        }
    }
}
