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
            Assert.That(capturedEntity.Id, Does.Match(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
            Assert.That(capturedEntity.Username, Is.EqualTo("IlarionPintilie"));
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
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(entity);

            Player player = playerService.Get("IlarionPintilie");

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Identifier, Is.EqualTo(entity.Id));
            Assert.That(player.Username, Is.EqualTo(entity.Username));
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

        // ── UpdateLastDeathLocation ────────────────────────────────────────────

        [Test]
        public void GivenAValidRequest_WhenUpdatingLastDeathLocation_ThenTheEntityIsUpdatedWithTheCorrectCoordinates()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            Coordinates location = BuildCoordinates();
            playerService.UpdateLastDeathLocation("IlarionPintilie", location);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.LastDeathLocation.World, Is.EqualTo(location.World));
            Assert.That(capturedEntity.LastDeathLocation.X, Is.EqualTo(location.X));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(location.Y));
            Assert.That(capturedEntity.LastDeathLocation.Z, Is.EqualTo(location.Z));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingLastDeathLocation_ThenLastDeathDTIsSet()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            playerService.UpdateLastDeathLocation("IlarionPintilie", BuildCoordinates());

            Assert.That(capturedEntity.LastDeathDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.LastDeathDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingLastDeathLocation_ThenUpdatedDTIsSet()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            playerService.UpdateLastDeathLocation("IlarionPintilie", BuildCoordinates());

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingLastDeathLocation_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            playerService.UpdateLastDeathLocation("IlarionPintilie", BuildCoordinates());

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingLastDeathLocation_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => playerService.UpdateLastDeathLocation("NonExistentUser", BuildCoordinates()),
                Throws.TypeOf<KeyNotFoundException>());
        }

        private static RegisterPlayerRequest BuildRegisterPlayerRequest() => new()
        {
            Username = "IlarionPintilie",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            Password = "NucilandiaPass1",
            IpAddress = "192.168.1.1",
            SkinUrl = "test.nucilandia.ro"
        };

        private static PlayerEntity BuildPlayerEntity() => new()
        {
            Id = "IlarionPintilie",
            Username = "IlarionPintilie",
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucilandiaPass1",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            UpdatedDT = null,
            IpAddress = "192.168.1.1",
            DiscordId = null,
            EmailAddress = "ilarion.pintilie@nucilandia.ro",
            LastSleptDT = "2012-09-05T00:00:00.0000000+00:00",
            LastDeathDT = null,
            LastDeathLocation = null,
            SkinUrl = "test.nucilandia.ro"
        };

        private static Coordinates BuildCoordinates() => new()
        {
            World = "world_nether",
            X = 613.5f,
            Y = 64.0f,
            Z = -873.25f
        };
    }
}
