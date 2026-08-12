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
        public void GivenAValidRequest_WhenHandlingPlayerDeath_ThenUpdateIsCalledForTheCorrectPlayer()
        {
            NotifyPlayerDeathRequest request = BuildNotifyPlayerDeathRequest();

            gameEventService.HandlePlayerDeath(request);

            playerServiceMock.Verify(
                service => service.Update(It.Is<UpdatePlayerRequest>(updateRequest =>
                    string.Equals(updateRequest.PlayerUsername, request.Player))),
                Times.Once);
        }

        [Test]
        public void GivenAValidRequest_WhenHandlingPlayerDeath_ThenUpdateIsCalledWithTheCorrectCoordinates()
        {
            NotifyPlayerDeathRequest request = BuildNotifyPlayerDeathRequest();

            gameEventService.HandlePlayerDeath(request);

            playerServiceMock.Verify(
                service => service.Update(It.Is<UpdatePlayerRequest>(updateRequest =>
                    string.Equals(updateRequest.LastDeathLocation.World, request.DeathLocation.World) &&
                    updateRequest.LastDeathLocation.X.Equals(request.DeathLocation.X) &&
                    updateRequest.LastDeathLocation.Y.Equals(request.DeathLocation.Y) &&
                    updateRequest.LastDeathLocation.Z.Equals(request.DeathLocation.Z) &&
                    updateRequest.LastDeathLocation.Pitch.Equals(request.DeathLocation.Pitch) &&
                    updateRequest.LastDeathLocation.Yaw.Equals(request.DeathLocation.Yaw))),
                Times.Once);
        }

        [Test]
        public void GivenAPlayerServiceException_WhenHandlingPlayerDeath_ThenTheExceptionIsPropagated()
        {
            playerServiceMock
                .Setup(service => service.Update(It.IsAny<UpdatePlayerRequest>()))
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
