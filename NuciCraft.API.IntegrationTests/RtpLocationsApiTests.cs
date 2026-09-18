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

        [Test]
        public async Task GivenExistingRtpLocations_WhenAddingLocationsAtTheDistanceLimits_ThenOnlyPermittedLocationsAreAdded()
        {
            HttpResponseMessage firstResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"forest\",\"world\":\"overworld\",\"x\":0,\"y\":64,\"z\":0}".CreateJsonContent());
            HttpResponseMessage globalBoundaryResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"plains\",\"world\":\"overworld\",\"x\":613,\"y\":64,\"z\":0}".CreateJsonContent());
            HttpResponseMessage biomeBoundaryResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"forest\",\"world\":\"overworld\",\"x\":700,\"y\":64,\"z\":0}".CreateJsonContent());
            HttpResponseMessage differentBiomeResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"plains\",\"world\":\"overworld\",\"x\":700,\"y\":64,\"z\":0}".CreateJsonContent());
            HttpResponseMessage differentWorldResponse = await client.PostAsync(
                "/rtplocations",
                "{\"biome\":\"forest\",\"world\":\"nether\",\"x\":0,\"y\":64,\"z\":0}".CreateJsonContent());

            Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(globalBoundaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(biomeBoundaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(differentBiomeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(differentWorldResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GivenNoMatchingRtpLocation_WhenFilteringLocations_ThenTheApiReturnsNotFound()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/rtplocations/random?biome=forest&world=overworld");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}