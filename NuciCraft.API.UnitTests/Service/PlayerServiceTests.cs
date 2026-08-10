using System;
using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

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
                .Setup(repository => repository.GetAll())
                .Returns([entity]);

            Player player = playerService.Get(new GetPlayerRequest { Username = "IlarionPintilie" });

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Identifier, Is.EqualTo(entity.Id));
            Assert.That(player.Username, Is.EqualTo(entity.Username));
            Assert.That(player.OfflineUUID, Is.EqualTo(entity.OfflineUUID));
            Assert.That(player.OnlineUUID, Is.EqualTo(entity.OnlineUUID));
            Assert.That(player.Password, Is.EqualTo(entity.Password));
            Assert.That(player.CreatedDT, Is.EqualTo(DateTimeOffset.Parse(entity.CreatedDT)));
            Assert.That(player.UpdatedDT, Is.Null);
            Assert.That(player.IpAddress, Is.EqualTo(entity.IpAddress));
            Assert.That(player.DiscordId, Is.EqualTo(entity.DiscordId));
            Assert.That(player.EmailAddress, Is.EqualTo(entity.EmailAddress));
            Assert.That(player.LastSleptDT, Is.EqualTo(DateTimeOffset.Parse(entity.LastSleptDT)));
            Assert.That(player.LastDeathDT, Is.Null);
            Assert.That(player.LastDeathLocation, Is.Null);
            Assert.That(player.BackLocation, Is.Not.Null);
            Assert.That(player.BackLocation.World, Is.EqualTo(entity.BackLocation.World));
            Assert.That(player.BackLocation.X, Is.EqualTo(entity.BackLocation.X));
            Assert.That(player.BackLocation.Y, Is.EqualTo(entity.BackLocation.Y));
            Assert.That(player.BackLocation.Z, Is.EqualTo(entity.BackLocation.Z));
            Assert.That(player.BackLocation.Pitch, Is.EqualTo(entity.BackLocation.Pitch));
            Assert.That(player.BackLocation.Yaw, Is.EqualTo(entity.BackLocation.Yaw));
            Assert.That(player.SkinUrl, Is.EqualTo(entity.SkinUrl));
        }

        [Test]
        public void GivenAValidIdentifier_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerEntity entity = BuildPlayerEntity();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([entity]);

            Player player = playerService.Get(new GetPlayerRequest { Identifier = entity.Id });

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Identifier, Is.EqualTo(entity.Id));
        }

        [Test]
        public void GivenAValidOfflineUUID_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerEntity entity = BuildPlayerEntity();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([entity]);

            Player player = playerService.Get(new GetPlayerRequest { OfflineUUID = entity.OfflineUUID });

            Assert.That(player, Is.Not.Null);
            Assert.That(player.OfflineUUID, Is.EqualTo(entity.OfflineUUID));
        }

        [Test]
        public void GivenAValidOnlineUUID_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerEntity entity = BuildPlayerEntity();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([entity]);

            Player player = playerService.Get(new GetPlayerRequest { OnlineUUID = entity.OnlineUUID });

            Assert.That(player, Is.Not.Null);
            Assert.That(player.OnlineUUID, Is.EqualTo(entity.OnlineUUID));
        }

        [Test]
        public void GivenNoMatchingPlayer_WhenGettingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => playerService.Get(new GetPlayerRequest { Username = "NonExistentUser" }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAPlayer_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.Get(new GetPlayerRequest { Username = "IlarionPintilie" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        // ── Update ─────────────────────────────────────────────────────────────

        [Test]
        public void GivenARequestWithAllFields_WhenUpdatingAPlayer_ThenAllFieldsAreApplied()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            UpdatePlayerRequest request = new()
            {
                Identifier = "IlarionPintilie",
                Username = "NewUsername",
                OnlineUUID = "11111111-0000-0000-0000-000000000000",
                Password = "NewPass",
                IpAddress = "10.0.0.1",
                DiscordId = "999",
                EmailAddress = "new@nucilandia.ro",
                LastSleptDT = "2026-01-01T00:00:00.0000000+00:00",
                LastDeathDT = "2026-06-01T00:00:00.0000000+00:00",
                LastDeathLocation = new() { World = "world_nether", X = 1.0f, Y = 2.0f, Z = 3.0f, Pitch = 4.0f, Yaw = 5.0f },
                SkinUrl = "new-skin.nucilandia.ro",
                BackLocation = new() { World = "world", X = 6.0f, Y = 7.0f, Z = 8.0f, Pitch = 9.0f, Yaw = 10.0f }
            };

            playerService.Update(request);

            Assert.That(capturedEntity.Username, Is.EqualTo("NewUsername"));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo("11111111-0000-0000-0000-000000000000"));
            Assert.That(capturedEntity.Password, Is.EqualTo("NewPass"));
            Assert.That(capturedEntity.IpAddress, Is.EqualTo("10.0.0.1"));
            Assert.That(capturedEntity.DiscordId, Is.EqualTo("999"));
            Assert.That(capturedEntity.EmailAddress, Is.EqualTo("new@nucilandia.ro"));
            Assert.That(capturedEntity.LastSleptDT, Is.EqualTo("2026-01-01T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastDeathDT, Is.EqualTo("2026-06-01T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastDeathLocation.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.LastDeathLocation.X, Is.EqualTo(1.0f));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(2.0f));
            Assert.That(capturedEntity.LastDeathLocation.Z, Is.EqualTo(3.0f));
            Assert.That(capturedEntity.LastDeathLocation.Pitch, Is.EqualTo(4.0f));
            Assert.That(capturedEntity.LastDeathLocation.Yaw, Is.EqualTo(5.0f));
            Assert.That(capturedEntity.SkinUrl, Is.EqualTo("new-skin.nucilandia.ro"));
            Assert.That(capturedEntity.BackLocation, Is.Not.Null);
            Assert.That(capturedEntity.BackLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BackLocation.X, Is.EqualTo(6.0f));
            Assert.That(capturedEntity.BackLocation.Y, Is.EqualTo(7.0f));
            Assert.That(capturedEntity.BackLocation.Z, Is.EqualTo(8.0f));
            Assert.That(capturedEntity.BackLocation.Pitch, Is.EqualTo(9.0f));
            Assert.That(capturedEntity.BackLocation.Yaw, Is.EqualTo(10.0f));
        }

        [Test]
        public void GivenARequestWithNullFields_WhenUpdatingAPlayer_ThenExistingValuesArePreserved()
        {
            PlayerEntity original = BuildPlayerEntity();
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Update(new UpdatePlayerRequest { Identifier = "IlarionPintilie" });

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
            Assert.That(capturedEntity.Password, Is.EqualTo(original.Password));
            Assert.That(capturedEntity.IpAddress, Is.EqualTo(original.IpAddress));
            Assert.That(capturedEntity.DiscordId, Is.EqualTo(original.DiscordId));
            Assert.That(capturedEntity.EmailAddress, Is.EqualTo(original.EmailAddress));
            Assert.That(capturedEntity.LastSleptDT, Is.EqualTo(original.LastSleptDT));
            Assert.That(capturedEntity.LastDeathDT, Is.EqualTo(original.LastDeathDT));
            Assert.That(capturedEntity.LastDeathLocation, Is.EqualTo(original.LastDeathLocation));
            Assert.That(capturedEntity.BackLocation, Is.EqualTo(original.BackLocation));
            Assert.That(capturedEntity.SkinUrl, Is.EqualTo(original.SkinUrl));
        }

        [Test]
        public void GivenARequestWithLastDeathLocationWhenEntityHasNone_WhenUpdatingAPlayer_ThenLastDeathLocationIsCreated()
        {
            PlayerEntity original = BuildPlayerEntity();
            original.LastDeathLocation = null;
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Update(new UpdatePlayerRequest
            {
                Identifier = "IlarionPintilie",
                LastDeathLocation = new() { World = "world_the_end", X = 0f, Y = 64f, Z = 0f }
            });

            Assert.That(capturedEntity.LastDeathLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastDeathLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(64f));
        }

        [Test]
        public void GivenARequestWithBackLocationWhenEntityHasNone_WhenUpdatingAPlayer_ThenBackLocationIsCreated()
        {
            PlayerEntity original = BuildPlayerEntity();
            original.BackLocation = null;
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.Update(new UpdatePlayerRequest
            {
                Identifier = "IlarionPintilie",
                BackLocation = new() { World = "world_the_end", X = 0f, Y = 64f, Z = 0f, Pitch = 11f, Yaw = 12f }
            });

            Assert.That(capturedEntity.BackLocation, Is.Not.Null);
            Assert.That(capturedEntity.BackLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.BackLocation.Y, Is.EqualTo(64f));
            Assert.That(capturedEntity.BackLocation.Pitch, Is.EqualTo(11f));
            Assert.That(capturedEntity.BackLocation.Yaw, Is.EqualTo(12f));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAPlayer_ThenUpdatedDTIsStamped()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            playerService.Update(new UpdatePlayerRequest { Identifier = "IlarionPintilie" });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAPlayer_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            playerService.Update(new UpdatePlayerRequest { Identifier = "IlarionPintilie" });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAPlayer_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => playerService.Update(new UpdatePlayerRequest { Identifier = "NonExistentPlayer" }),
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
            Assert.That(capturedEntity.LastDeathLocation.Pitch, Is.EqualTo(location.Pitch));
            Assert.That(capturedEntity.LastDeathLocation.Yaw, Is.EqualTo(location.Yaw));
        }

        [Test]
        public void GivenAPlayerWithABackLocation_WhenUpdatingLastDeathLocation_ThenTheBackLocationIsPreserved()
        {
            PlayerEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("IlarionPintilie"))
                .Returns(BuildPlayerEntity());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerEntity>()))
                .Callback<PlayerEntity>(entity => capturedEntity = entity);

            playerService.UpdateLastDeathLocation("IlarionPintilie", BuildCoordinates());

            Assert.That(capturedEntity.BackLocation, Is.Not.Null);
            Assert.That(capturedEntity.BackLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BackLocation.X, Is.EqualTo(100.5f));
            Assert.That(capturedEntity.BackLocation.Y, Is.EqualTo(70.0f));
            Assert.That(capturedEntity.BackLocation.Z, Is.EqualTo(-25.25f));
            Assert.That(capturedEntity.BackLocation.Pitch, Is.EqualTo(45.0f));
            Assert.That(capturedEntity.BackLocation.Yaw, Is.EqualTo(90.0f));
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
            BackLocation = new() { World = "world", X = 100.5f, Y = 70.0f, Z = -25.25f, Pitch = 45.0f, Yaw = 90.0f },
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
