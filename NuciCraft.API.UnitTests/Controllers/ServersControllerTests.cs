using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Controllers;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class ServersControllerTests
    {
        private Mock<IServerStatusService> serverStatusServiceMock;
        private ServerSettings serverSettings;
        private ServersController controller;

        [SetUp]
        public void SetUp()
        {
            serverStatusServiceMock = new Mock<IServerStatusService>();
            serverSettings = new ServerSettings();
            controller = new ServersController(
                serverStatusServiceMock.Object,
                serverSettings,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [TestCase("Testy McTestface", "example.invalid", 613, 873)]
        [TestCase("DummyUser", "localhost", 25565, 19132)]
        [TestCase("Testy McTestface", "127.0.0.1", 1, 65535)]
        [TestCase("Testy McTestface", "::1", 65535, 1)]
        [TestCase("Testy McTestface", "example.invalid", 613, 613)]
        [TestCase("", "", 0, 0)]
        [TestCase(null, null, 0, 0)]
        public void GivenServerSettings_WhenRetrievingServerInformation_ThenTheValuesArePreserved(
            string name,
            string hostname,
            int javaEditionPort,
            int bedrockEditionPort)
        {
            serverSettings.Name = name;
            serverSettings.Hostname = hostname;
            serverSettings.JavaEditionPort = javaEditionPort;
            serverSettings.BedrockEditionPort = bedrockEditionPort;

            OkObjectResult result = controller.Get() as OkObjectResult;

            Assert.That(result, Is.Not.Null);

            NuciApiContentResponse<GetServerResponse> response =
                result.Value as NuciApiContentResponse<GetServerResponse>;

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Content.Name, Is.EqualTo(name));
            Assert.That(response.Content.Hostname, Is.EqualTo(hostname));
            Assert.That(response.Content.JavaEditionPort, Is.EqualTo(javaEditionPort));
            Assert.That(response.Content.BedrockEditionPort, Is.EqualTo(bedrockEditionPort));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(613)]
        [TestCase(int.MaxValue)]
        public void GivenALivePlayerCount_WhenRetrievingServerInformation_ThenTheCountIsReturned(
            int onlinePlayersCount)
        {
            serverStatusServiceMock
                .Setup(service => service.GetOnlinePlayersCount())
                .Returns(onlinePlayersCount);

            OkObjectResult result = controller.Get() as OkObjectResult;
            NuciApiContentResponse<GetServerResponse> response =
                result.Value as NuciApiContentResponse<GetServerResponse>;

            Assert.That(response.Content.OnlinePlayersCount, Is.EqualTo(onlinePlayersCount));
            serverStatusServiceMock.Verify(service => service.GetOnlinePlayersCount(), Times.Once);
        }
    }
}