using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class RtpLocationsApiTests
    {
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
        public async Task GivenAnRtpLocation_WhenCreatingAndFilteringIt_ThenTheLocationIsReturned()
        {
            HttpResponseMessage createResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"forest\",\"world\":\"overworld\",\"x\":1024,\"y\":64,\"z\":2048}".CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync(
                "/rtplocations/random?biome=forest&world=overworld");
            string getBody = await getResponse.Content.ReadAsStringAsync();

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getBody, Does.Contain("forest"));
        }
    }
}