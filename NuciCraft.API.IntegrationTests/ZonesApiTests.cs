using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ZonesApiTests
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
        public async Task GivenAZone_WhenCreatingReadingListingFilteringPatchingAndDeletingIt_ThenEveryRouteSucceeds()
        {
            await CreateWorldAndZoneType();

            HttpResponseMessage createResponse = await client.PostAsync(
                "/zones",
                "{\"id\":\"cornova\",\"type\":\"city\",\"world\":\"overworld\",\"bounds\":{\"firstCorner\":{\"world\":\"overworld\",\"x\":0,\"y\":0,\"z\":0},\"secondCorner\":{\"world\":\"overworld\",\"x\":32,\"y\":128,\"z\":32}}}".CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync("/zones/cornova");
            HttpResponseMessage listResponse = await client.GetAsync("/zones");
            HttpResponseMessage typeResponse = await client.GetAsync("/zones?type=CiTy");
            HttpResponseMessage categoryResponse = await client.GetAsync("/zones/by-category/settlement");
            string categoryResponseBody = await categoryResponse.Content.ReadAsStringAsync();
            HttpResponseMessage coordinatesResponse = await client.GetAsync(
                "/zones/by-coordinates?world=overworld&x=16&y=64&z=16");
            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/zones/cornova",
                "{\"population\":613}".CreateJsonContent());
            HttpResponseMessage deleteResponse = await client.DeleteAsync("/zones/cornova");

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(typeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(categoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await typeResponse.Content.ReadAsStringAsync(), Does.Contain("\"identifier\":\"cornova\""));
            Assert.That(categoryResponseBody, Does.Contain("\"identifier\":\"cornova\""));
            Assert.That(coordinatesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GivenZoneCoordinatesOutsideTheBounds_WhenFilteringZones_ThenNoZoneIsReturned()
        {
            await CreateWorldAndZoneType();
            await client.PostAsync(
                "/zones",
                "{\"id\":\"cornova\",\"type\":\"city\",\"world\":\"overworld\",\"bounds\":{\"firstCorner\":{\"world\":\"overworld\",\"x\":0,\"y\":0,\"z\":0},\"secondCorner\":{\"world\":\"overworld\",\"x\":32,\"y\":128,\"z\":32}}}".CreateJsonContent());

            HttpResponseMessage response = await client.GetAsync(
                "/zones/by-coordinates?world=overworld&x=64&y=64&z=64");
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Does.Contain("\"zoneIdentifiers\":[]"));
        }

        [Test]
        public async Task GivenInvalidZoneDependenciesAndBounds_WhenCreatingAZone_ThenTheApiReturnsTheRelevantError()
        {
            HttpResponseMessage missingWorldResponse = await client.PostAsync(
                "/zones",
                "{\"id\":\"cornova\",\"type\":\"city\",\"world\":\"absent\",\"bounds\":{\"firstCorner\":{\"world\":\"absent\",\"x\":0,\"y\":0,\"z\":0},\"secondCorner\":{\"world\":\"absent\",\"x\":32,\"y\":128,\"z\":32}}}".CreateJsonContent());
            await client.PostAsync(
                "/worlds",
                "{\"id\":\"overworld\"}".CreateJsonContent());
            HttpResponseMessage missingTypeResponse = await client.PostAsync(
                "/zones",
                "{\"id\":\"cornova\",\"type\":\"absent\",\"world\":\"overworld\",\"bounds\":{\"firstCorner\":{\"world\":\"overworld\",\"x\":0,\"y\":0,\"z\":0},\"secondCorner\":{\"world\":\"overworld\",\"x\":32,\"y\":128,\"z\":32}}}".CreateJsonContent());
            await client.PostAsync(
                "/zonetypes",
                "{\"id\":\"city\"}".CreateJsonContent());
            HttpResponseMessage mixedBoundsResponse = await client.PostAsync(
                "/zones",
                "{\"id\":\"cornova\",\"type\":\"city\",\"world\":\"overworld\",\"bounds\":{\"firstCorner\":{\"world\":\"overworld\",\"x\":0,\"y\":0,\"z\":0},\"secondCorner\":{\"world\":\"nether\",\"x\":32,\"y\":128,\"z\":32}}}".CreateJsonContent());

            Assert.That(missingWorldResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(missingTypeResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(mixedBoundsResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        private async Task CreateWorldAndZoneType()
        {
            await client.PostAsync(
                "/worlds",
                "{\"id\":\"overworld\"}".CreateJsonContent());
            await client.PostAsync(
                "/zonetypes",
                "{\"id\":\"city\",\"categories\":[\"settlement\",\"civilian\"]}".CreateJsonContent());
        }
    }
}