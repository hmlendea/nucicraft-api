using System;
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
    public sealed class GetHomeResponseTests
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        [Test]
        public void GivenAHome_WhenCreatingTheResponse_ThenTheHomePropertiesAreProjected()
        {
            DateTimeOffset createdDT = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
            Home home = new()
            {
                Identifier = "test_home",
                CreatedDT = createdDT,
                Name = new() { English = "Test Home" },
                Player = "player1"
            };

            GetHomeResponse response = new(home);

            Assert.That(response.Identifier, Is.EqualTo(home.Identifier));
            Assert.That(response.CreatedDT, Is.EqualTo(home.CreatedDT));
            Assert.That(response.UpdatedDT, Is.EqualTo(home.UpdatedDT));
            Assert.That(response.Name, Is.SameAs(home.Name));
            Assert.That(response.Player, Is.EqualTo(home.Player));
            Assert.That(response.Location, Is.SameAs(home.Location));
        }

        [Test]
        public void GivenAGetHomeResponse_WhenSerialising_ThenTheIdentifierIsSerialisedAsId()
        {
            NuciApiContentResponse<GetHomeResponse> response = new(new GetHomeResponse(new()
            {
                Identifier = "test_home",
                CreatedDT = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
                Player = "player1"
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
                Is.EqualTo(["id", "createdDT", "updatedDT", "name", "player", "location"]));
            Assert.That(
                contentElement.GetProperty("id").GetString(),
                Is.EqualTo("test_home"));
            Assert.That(
                contentElement.GetProperty("player").GetString(),
                Is.EqualTo("player1"));
        }
    }
}
