using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class MobsApiTests
    {
        private static string[] SupportedMobTypes =>
        [
            "ender_dragon",
            "cow",
            "pig",
            "evoker",
            "illusioner",
            "pillager",
            "vindicator",
            "villager",
            "wandering_trader"
        ];

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

        [TestCaseSource(nameof(SupportedMobTypes))]
        public async Task GivenASupportedMobType_WhenGeneratingARandomName_ThenTheStubbedNameIsReturned(
            string mobType)
        {
            HttpResponseMessage response = await client.GetAsync($"/mobs/{mobType}/random-name");
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Does.Contain("Ilarion"));
        }

        [Test]
        public async Task GivenAnUnsupportedMobType_WhenGeneratingARandomName_ThenTheApiReturnsNotImplemented()
        {
            HttpResponseMessage response = await client.GetAsync("/mobs/unknown/random-name");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
        }
    }
}