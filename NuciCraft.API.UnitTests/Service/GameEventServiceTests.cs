using System;

using Moq;
using NUnit.Framework;

using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

using NuciLog.Core;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class GameEventServiceTests
    {
        Mock<IPlayerService> playerServiceMock;
        Mock<ILogger> loggerMock;
        GameEventService gameEventService;

        [SetUp]
        public void SetUp()
        {
            playerServiceMock = new Mock<IPlayerService>();
            loggerMock = new Mock<ILogger>();
            gameEventService = new GameEventService(playerServiceMock.Object, loggerMock.Object);
        }

        // ── HandlePlayerDeath ─────────────────────────────────────────────────

        [Test]
        public void GivenAValidRequest_WhenHandlingPlayerDeath_ThenUpdateLastDeathLocationIsCalledForTheCorrectPlayer()
        {
            NotifyPlayerDeathRequest request = BuildNotifyPlayerDeathRequest();

            gameEventService.HandlePlayerDeath(request);

            playerServiceMock.Verify(
                service => service.UpdateLastDeathLocation(
                    request.Player,
                    It.IsAny<Coordinates>()),
                Times.Once);
        }

        [Test]
        public void GivenAValidRequest_WhenHandlingPlayerDeath_ThenUpdateLastDeathLocationIsCalledWithTheCorrectCoordinates()
        {
            NotifyPlayerDeathRequest request = BuildNotifyPlayerDeathRequest();

            gameEventService.HandlePlayerDeath(request);

            playerServiceMock.Verify(
                service => service.UpdateLastDeathLocation(
                    It.IsAny<string>(),
                    request.DeathLocation),
                Times.Once);
        }

        [Test]
        public void GivenAPlayerServiceException_WhenHandlingPlayerDeath_ThenTheExceptionIsPropagated()
        {
            playerServiceMock
                .Setup(service => service.UpdateLastDeathLocation(It.IsAny<string>(), It.IsAny<Coordinates>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => gameEventService.HandlePlayerDeath(BuildNotifyPlayerDeathRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        private static NotifyPlayerDeathRequest BuildNotifyPlayerDeathRequest() => new()
        {
            Player = "IlarionPintilie",
            DeathLocation = new Coordinates
            {
                World = "world_the_end",
                X = 613.5f,
                Y = 64.0f,
                Z = -873.25f
            }
        };
    }
}
