using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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
                "{\"id\":\"city\",\"categories\":[\"settlement\",\"civilian\"]}".CreateJsonContent());
            HttpResponseMessage getResponse = await client.GetAsync("/zonetypes/city");
            HttpResponseMessage listResponse = await client.GetAsync("/zonetypes");
            HttpResponseMessage patchResponse = await client.PatchAsync(
                "/zonetypes/city",
                "{\"id\":\"ignored\",\"categories\":[\"capital\",\"fortified\"]}".CreateJsonContent());
            HttpResponseMessage patchedGetResponse = await client.GetAsync("/zonetypes/city");

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(patchedGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            JsonDocument getResponseDocument = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
            JsonElement getContent = getResponseDocument.RootElement.GetProperty("content");
            JsonDocument listResponseDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
            JsonElement listZoneType = listResponseDocument.RootElement
                .GetProperty("content")
                .GetProperty("zoneTypes")
                .EnumerateArray()
                .Single(zoneType => zoneType.GetProperty("identifier").GetString() == "city");
            JsonDocument patchedGetResponseDocument = JsonDocument.Parse(
                await patchedGetResponse.Content.ReadAsStringAsync());
            JsonElement patchedGetContent = patchedGetResponseDocument.RootElement.GetProperty("content");

            Assert.That(
                getContent.GetProperty("categories").EnumerateArray().Select(category => category.GetString()),
                Is.EqualTo(["settlement", "civilian"]));
            Assert.That(
                listZoneType.GetProperty("categories").EnumerateArray().Select(category => category.GetString()),
                Is.EqualTo(["settlement", "civilian"]));
            Assert.That(
                patchedGetContent.GetProperty("categories").EnumerateArray().Select(category => category.GetString()),
                Is.EqualTo(["capital", "fortified"]));
        }
    }
}