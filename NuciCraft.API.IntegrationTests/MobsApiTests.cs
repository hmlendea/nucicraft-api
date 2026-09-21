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
    public sealed class MobsApiTests
    {
        private static int RequestedNameCount => 4;

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
        public async Task GivenASupportedMobTypeAndNoCount_WhenGeneratingRandomNames_ThenOneNameIsReturned(
            string mobType)
        {
            HttpResponseMessage response = await client.GetAsync($"/mobs/{mobType}/random-name");
            string responseBody = await response.Content.ReadAsStringAsync();
            string[] generatedNames = ExtractGeneratedNames(responseBody);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(generatedNames, Has.Length.EqualTo(1));
            Assert.That(generatedNames, Is.All.EqualTo("Ilarion"));
        }

        [Test]
        public async Task GivenACount_WhenGeneratingRandomNames_ThenThatManyNamesAreReturned()
        {
            HttpResponseMessage response = await client.GetAsync(
                $"/mobs/villager/random-name?count={RequestedNameCount}");
            string responseBody = await response.Content.ReadAsStringAsync();
            string[] generatedNames = ExtractGeneratedNames(responseBody);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(generatedNames, Has.Length.EqualTo(RequestedNameCount));
            Assert.That(generatedNames, Is.All.EqualTo("Ilarion"));
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(100001)]
        public async Task GivenAnInvalidCount_WhenGeneratingRandomNames_ThenBadRequestIsReturned(
            int count)
        {
            HttpResponseMessage response = await client.GetAsync(
                $"/mobs/villager/random-name?count={count}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GivenAnUnsupportedMobType_WhenGeneratingARandomName_ThenTheApiReturnsNotImplemented()
        {
            HttpResponseMessage response = await client.GetAsync("/mobs/unknown/random-name");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
        }

        private static string[] ExtractGeneratedNames(string responseBody)
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);

            return document.RootElement
                .GetProperty("content")
                .GetProperty("names")
                .EnumerateArray()
                .Select(nameElement => nameElement.GetString()!)
                .ToArray();
        }
    }
}