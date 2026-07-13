using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciLog.Core;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class PlayerServiceTests
    {
        Mock<IFileRepository<PlayerEntity>> repositoryMock;
        Mock<ILogger> loggerMock;
        PlayerService playerService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<PlayerEntity>>();
            loggerMock = new Mock<ILogger>();
            playerService = new PlayerService(repositoryMock.Object, loggerMock.Object);
        }

        // ── Register ──────────────────────────────────────────────────────────

        [Test]
        public void GivenAValidRequest_WhenRegistering_ThenTheEntityIsAddedToTheRepository()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Id, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo("87300000-0000-0000-0000-000000000000"));
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass1"));
            Assert.That(capturedEntity.IpAddress, Is.EqualTo("192.168.1.1"));
            Assert.That(capturedEntity.SkinUrl, Is.EqualTo("test.nucilandia.ro"));
        }

        [Test]
        public void GivenAValidRequest_WhenRegistering_ThenTheOfflineUUIDIsCalculated()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity.OfflineUUID, Is.Not.Null);
            Assert.That(capturedEntity.OfflineUUID, Is.Not.Empty);
            Assert.That(capturedEntity.OfflineUUID, Does.Match(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
        }

        [Test]
        public void GivenAValidRequestWithExplicitCreatedDT_WhenRegistering_ThenCreatedDTIsParsedAndStored()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Register(request);

            DateTimeOffset expectedCreatedDT = DateTimeOffset.Parse("2012-09-05T00:00:00.0000000+00:00");
            DateTimeOffset actualCreatedDT = DateTimeOffset.Parse(capturedEntity.CreatedDT);

            Assert.That(actualCreatedDT, Is.EqualTo(expectedCreatedDT));
        }

        [Test]
        public void GivenAValidRequestWithNoCreatedDT_WhenRegistering_ThenCreatedDTIsSetToCurrentTime()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            request.CreatedDT = null;
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            DateTimeOffset registrationTime = DateTimeOffset.Now;
            playerService.Register(request);

            DateTimeOffset actualCreatedDT = DateTimeOffset.Parse(capturedEntity.CreatedDT);

            Assert.That(actualCreatedDT, Is.GreaterThanOrEqualTo(registrationTime));
        }

        [Test]
        public void GivenAValidRequest_WhenRegistering_ThenSaveChangesIsInvoked()
        {
            playerService.Register(BuildRegisterPlayerRequest());

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenRegistering_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerEntity>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.Register(BuildRegisterPlayerRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        // ── Get ────────────────────────────────────────────────────────────────

        [Test]
        public void GivenAValidUsername_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerEntity entity = BuildPlayerEntity();

            repositoryMock
                .Setup(repository => repository.Get("Ilarion Pintilie"))
                .Returns(entity);

            Player player = playerService.Get("Ilarion Pintilie");

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Username, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(player.OfflineUUID, Is.EqualTo(entity.OfflineUUID));
            Assert.That(player.OnlineUUID, Is.EqualTo(entity.OnlineUUID));
            Assert.That(player.Password, Is.EqualTo(entity.Password));
            Assert.That(player.IpAddress, Is.EqualTo(entity.IpAddress));
            Assert.That(player.SkinUrl, Is.EqualTo(entity.SkinUrl));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAPlayer_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => playerService.Get("NonExistentUser"),
                Throws.TypeOf<KeyNotFoundException>());
        }

        private static RegisterPlayerRequest BuildRegisterPlayerRequest() => new()
        {
            Username = "Ilarion Pintilie",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            Password = "NucilandiaPass1",
            IpAddress = "192.168.1.1",
            SkinUrl = "test.nucilandia.ro"
        };

        private static PlayerEntity BuildPlayerEntity() => new()
        {
            Id = "Ilarion Pintilie",
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucilandiaPass1",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            UpdatedDT = null,
            IpAddress = "192.168.1.1",
            DiscordId = null,
            SkinUrl = "test.nucilandia.ro"
        };
    }
}
