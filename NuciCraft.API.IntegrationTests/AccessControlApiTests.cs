using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class AccessControlApiTests
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
        public async Task GivenAnIpAddress_WhenCheckingIt_ThenTheAccessControlResultIsReturned()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip?ipAddress=8.8.8.8");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"success\":true"));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"countryCode\":\"CN\""));
            Assert.That(body, Does.Contain("\"deniedReason\":\"Access is not permitted from this country.\""));
        }

        [Test]
        public async Task GivenAnAllowedUsername_WhenCheckingIt_ThenTheUsernameIsPermitted()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-username?username=TestyMcTest445");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"success\":true"));
            Assert.That(body, Does.Contain("\"isAllowed\":true"));
            Assert.That(body, Does.Contain("\"deniedReason\":null"));
        }

        [Test]
        public async Task GivenADeniedUsername_WhenCheckingIt_ThenTheUsernameIsRejected()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-username?username=Player");
            string body = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"success\":true"));
            Assert.That(body, Does.Contain("\"isAllowed\":false"));
            Assert.That(body, Does.Contain("\"deniedReason\":\"This username is not permitted.\""));
        }

        [Test]
        public async Task GivenAMissingIpAddress_WhenCheckingIt_ThenTheRequestIsRejected()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-ip");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GivenAMissingUsername_WhenCheckingIt_ThenTheRequestIsRejected()
        {
            HttpResponseMessage response = await client.GetAsync(
                "/accesscontrol/check-username");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
