using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CountriesApiTests
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
        public async Task GivenACountry_WhenCreatingReadingListingAndPatchingIt_ThenTheChangesPersist()
        {
            HttpResponseMessage createResponse = await client.PostAsync(
                "/countries",
                "{\"id\":\"nucilandia\",\"leader\":\"Ilarion Pintilie\"}".CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync("/countries/nucilandia");
            HttpResponseMessage listResponse = await client.GetAsync("/countries");
            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/countries/nucilandia",
                "{\"leader\":\"Vasile Ciupitu\"}".CreateJsonContent());
            HttpResponseMessage updatedGetResponse = await client.GetAsync("/countries/nucilandia");
            string updatedGetBody = await updatedGetResponse.Content.ReadAsStringAsync();

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedGetBody, Does.Contain("Vasile Ciupitu"));
        }
    }
}