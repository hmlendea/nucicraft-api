using System;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class ServerStatusServiceTests
    {
        private ServerSettings settings;
        private ServerStatusService service;

        [SetUp]
        public void SetUp()
        {
            settings = new ServerSettings
            {
                Hostname = "example.invalid",
                JavaEditionPort = 613
            };
            service = new ServerStatusService(settings);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public async Task GivenAMissingHostname_WhenRetrievingThePlayerCount_ThenTheConfigurationIsRejected(
            string hostname)
        {
            settings.Hostname = hostname;

            await Assert.ThatAsync(
                async () => await service.GetOnlinePlayersCountAsync(),
                Throws.InstanceOf<ArgumentException>());
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(65536)]
        [TestCase(int.MaxValue)]
        public async Task GivenAnInvalidJavaPort_WhenRetrievingThePlayerCount_ThenTheConfigurationIsRejected(
            int port)
        {
            settings.JavaEditionPort = port;

            await Assert.ThatAsync(
                async () => await service.GetOnlinePlayersCountAsync(),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}