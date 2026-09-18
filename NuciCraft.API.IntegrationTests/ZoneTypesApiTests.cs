using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ZoneTypesApiTests
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
        public async Task GivenAZoneType_WhenCreatingReadingListingAndPatchingIt_ThenTheChangesPersist()
        {
            HttpResponseMessage createResponse = await client.PostAsync(
                "/zonetypes",
                "{\"id\":\"city\"}".CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync("/zonetypes/city");
            HttpResponseMessage listResponse = await client.GetAsync("/zonetypes");
            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/zonetypes/city",
                "{\"id\":\"ignored\"}".CreateJsonContent());

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}