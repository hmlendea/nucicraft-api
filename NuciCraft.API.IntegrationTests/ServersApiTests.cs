using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ServersApiTests
    {
        private static string ServerRoute => "/server";

        private ApiTestHost testHost = null!;
        private HttpClient client = null!;

        [SetUp]
        public void SetUp()
        {
            testHost = new ApiTestHost();
            client = testHost.CreateAuthorisedClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            testHost.Dispose();
        }

        [Test]
        public async Task GivenConfiguredServerInformation_WhenRetrievingIt_ThenAllConfiguredValuesAreReturned()
        {
            using HttpResponseMessage response = await client.GetAsync(ServerRoute);
            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), responseBody);

            JsonElement content = document.RootElement.GetProperty("content");

            Assert.That(content.GetProperty("name").GetString(), Is.EqualTo("Testy McTestface"));
            Assert.That(content.GetProperty("hostname").GetString(), Is.EqualTo("example.invalid"));
            Assert.That(content.GetProperty("javaEditionPort").GetInt32(), Is.EqualTo(613));
            Assert.That(content.GetProperty("bedrockEditionPort").GetInt32(), Is.EqualTo(873));
            Assert.That(content.EnumerateObject(), Has.Exactly(4).Items);
        }

        [TestCase(null)]
        [TestCase("DummyUser")]
        public async Task GivenMissingOrInvalidAuthorisation_WhenRetrievingServerInformation_ThenAccessIsDenied(
            string? apiKey)
        {
            client.DefaultRequestHeaders.Authorization = null;

            if (apiKey is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            using HttpResponseMessage response = await client.GetAsync(ServerRoute);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}