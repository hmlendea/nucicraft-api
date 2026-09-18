using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class WorldsApiTests
    {
        private static JsonSerializerOptions JsonSerializerOptions => new(JsonSerializerDefaults.Web);

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
        public async Task GivenANewWorld_WhenCreatingReadingAndPatchingIt_ThenTheChangesPersist()
        {
            HttpResponseMessage createResponse = await client.PostAsync(
                "/worlds",
                CreateJsonContent("{\"id\":\"overworld\",\"hasWebMap\":true,\"type\":\"overworld\"}"));
            string createBody = await createResponse.Content.ReadAsStringAsync();

            Assert.That(
                createResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK),
                createBody);

            HttpResponseMessage firstGetResponse = await client.GetAsync("/worlds/overworld");
            string firstGetBody = await firstGetResponse.Content.ReadAsStringAsync();
            using JsonDocument firstGetDocument = JsonDocument.Parse(firstGetBody);

            Assert.That(firstGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(
                firstGetDocument.RootElement.GetProperty("content").GetProperty("id").GetString(),
                Is.EqualTo("overworld"));

            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/worlds/overworld",
                CreateJsonContent("{\"hasWebMap\":false}"));

            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            HttpResponseMessage secondGetResponse = await client.GetAsync("/worlds/overworld");
            string secondGetBody = await secondGetResponse.Content.ReadAsStringAsync();
            using JsonDocument secondGetDocument = JsonDocument.Parse(secondGetBody);

            Assert.That(secondGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(
                secondGetDocument.RootElement.GetProperty("content").GetProperty("hasWebMap").GetBoolean(),
                Is.False);
        }

        private static StringContent CreateJsonContent(string content)
            => new(content, Encoding.UTF8, "application/json");
    }
}