using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ServersApiTests
    {
        private static string ServerRoute => "/server";

        private ApiTestHost testHost = null!;
        private HttpClient client = null!;
        private Mock<IServerStatusService> serverStatusServiceMock = null!;

        [SetUp]
        public void SetUp()
        {
            testHost = new ApiTestHost();
            client = testHost.CreateAuthorisedClient();
            serverStatusServiceMock = Mock.Get(testHost.Services.GetRequiredService<IServerStatusService>());
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            testHost.Dispose();
        }

        [TestCase(0)]
        [TestCase(42)]
        [TestCase(613)]
        public async Task GivenConfiguredServerInformation_WhenRetrievingIt_ThenAllConfiguredValuesAreReturned(
            int onlinePlayersCount)
        {
            serverStatusServiceMock
                .Setup(service => service.GetOnlinePlayersCount())
                .Returns(onlinePlayersCount);

            using HttpResponseMessage response = await client.GetAsync(ServerRoute);
            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), responseBody);

            JsonElement content = document.RootElement.GetProperty("content");

            Assert.That(content.GetProperty("name").GetString(), Is.EqualTo("Testy McTestface"));
            Assert.That(content.GetProperty("hostname").GetString(), Is.EqualTo("example.invalid"));
            Assert.That(content.GetProperty("javaEditionPort").GetInt32(), Is.EqualTo(613));
            Assert.That(content.GetProperty("bedrockEditionPort").GetInt32(), Is.EqualTo(873));
            Assert.That(content.GetProperty("onlinePlayersCount").GetInt32(), Is.EqualTo(onlinePlayersCount));
            Assert.That(content.EnumerateObject(), Has.Exactly(5).Items);
        }

        [Test]
        public async Task GivenAnUnavailableJavaServer_WhenRetrievingServerInformation_ThenZeroIsReturned()
        {
            using TcpListener listener = new(IPAddress.Loopback, 0);
            listener.Start();
            IPEndPoint endpoint = (IPEndPoint)listener.LocalEndpoint;
            listener.Stop();
            ServerStatusService service = new(new ServerSettings
            {
                Hostname = endpoint.Address.ToString(),
                JavaEditionPort = endpoint.Port
            });
            serverStatusServiceMock
                .Setup(statusService => statusService.GetOnlinePlayersCount())
                .Returns(service.GetOnlinePlayersCount);

            using HttpResponseMessage response = await client.GetAsync(ServerRoute);
            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), responseBody);
            Assert.That(
                document.RootElement.GetProperty("content").GetProperty("onlinePlayersCount").GetInt32(),
                Is.Zero);
        }

        [Test]
        public async Task GivenAnInvalidJavaStatus_WhenRetrievingServerInformation_ThenAnErrorIsReturned()
        {
            serverStatusServiceMock
                .Setup(service => service.GetOnlinePlayersCount())
                .Throws(new InvalidOperationException("The Java status query failed."));

            using HttpResponseMessage response = await client.GetAsync(ServerRoute);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }

        [TestCase(null)]
        [TestCase("DummyUser")]
        public async Task GivenMissingOrInvalidAuthorisation_WhenRetrievingServerInformation_ThenAccessIsDenied(
            string? apiKey)
        {
            client.DefaultRequestHeaders.Authorization = null;

            if (apiKey is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            using HttpResponseMessage response = await client.GetAsync(ServerRoute);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            serverStatusServiceMock.Verify(service => service.GetOnlinePlayersCount(), Times.Never);
        }
    }
}