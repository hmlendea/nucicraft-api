using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ApiErrorResponseTests
    {
        private ApiTestHost testHost = null!;

        [SetUp]
        public void SetUp() => testHost = new ApiTestHost();

        [TearDown]
        public void TearDown() => testHost.Dispose();

        [Test]
        public async Task GivenAnUnauthorisedRequest_WhenReadingWorlds_ThenTheApiReturnsUnauthorised()
        {
            HttpClient client = testHost.CreateClient();
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

            HttpResponseMessage response = await client.GetAsync("/worlds");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GivenAnInvalidApiKey_WhenReadingWorlds_ThenTheApiReturnsUnauthorised()
        {
            HttpClient client = testHost.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-api-key");
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

            HttpResponseMessage response = await client.GetAsync("/worlds");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GivenARequestWithoutARequiredProperty_WhenCreatingAWorld_ThenTheApiReturnsBadRequest()
        {
            HttpClient client = testHost.CreateAuthorisedClient();

            HttpResponseMessage response = await client.PostAsync(
                "/worlds",
                "{}".CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GivenAnAbsentWorld_WhenReadingIt_ThenTheApiReturnsNotFound()
        {
            HttpClient client = testHost.CreateAuthorisedClient();

            HttpResponseMessage response = await client.GetAsync("/worlds/absent");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GivenMalformedJson_WhenCreatingAWorld_ThenTheApiReturnsBadRequest()
        {
            HttpClient client = testHost.CreateAuthorisedClient();

            HttpResponseMessage response = await client.PostAsync(
                "/worlds",
                "{\"id\":\"overworld\"".CreateJsonContent());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/problem+json"));
        }
    }
}