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
        Mock<IFileRepository<PlayerDataObject>> repositoryMock;
        Mock<ILogger> loggerMock;
        PlayerService playerService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<PlayerDataObject>>();
            loggerMock = new Mock<ILogger>();
            playerService = new PlayerService(repositoryMock.Object, loggerMock.Object);
        }

        // ── Register ──────────────────────────────────────────────────────────

        [Test]
        public void GivenAValidRequest_WhenRegistering_ThenTheEntityIsAddedToTheRepository()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Id, Does.Match(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
            Assert.That(capturedEntity.Username, Is.EqualTo("IlarionPintilie"));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo("87300000-0000-0000-0000-000000000000"));
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass1"));
            Assert.That(capturedEntity.IpAddress, Is.EqualTo("192.168.1.1"));
            Assert.That(capturedEntity.Settings, Is.Not.Null);
            Assert.That(capturedEntity.Settings.Localisation, Is.EqualTo("romanian"));
            Assert.That(capturedEntity.Settings.SkinUrl, Is.Null);
            Assert.That(capturedEntity.LogoutLocation, Is.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenRegistering_ThenTheOfflineUUIDIsCalculated()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity.OfflineUUID, Is.Not.Null);
            Assert.That(capturedEntity.OfflineUUID, Is.Not.Empty);
            Assert.That(capturedEntity.OfflineUUID, Does.Match(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
        }

        [Test]
        public void GivenAValidRequestWithExplicitCreatedDT_WhenRegistering_ThenCreatedDTIsParsedAndStored()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

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
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

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
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.Register(BuildRegisterPlayerRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        // ── Get ────────────────────────────────────────────────────────────────

        [Test]
        public void GivenAValidUsername_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerDataObject entity = BuildPlayerDataObject();

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
            Assert.That(player.LogoutLocation, Is.Not.Null);
            Assert.That(player.LogoutLocation.World, Is.EqualTo(entity.LogoutLocation.World));
            Assert.That(player.LogoutLocation.X, Is.EqualTo(entity.LogoutLocation.X));
            Assert.That(player.LogoutLocation.Y, Is.EqualTo(entity.LogoutLocation.Y));
            Assert.That(player.LogoutLocation.Z, Is.EqualTo(entity.LogoutLocation.Z));
            Assert.That(player.LogoutLocation.Pitch, Is.EqualTo(entity.LogoutLocation.Pitch));
            Assert.That(player.LogoutLocation.Yaw, Is.EqualTo(entity.LogoutLocation.Yaw));
            Assert.That(player.Settings, Is.Not.Null);
            Assert.That(player.Settings.AutomaticSaplingReplantingIsEnabled, Is.EqualTo(entity.Settings.AutomaticSaplingReplantingIsEnabled));
            Assert.That(player.Settings.PrivateMessagesAreEnabled, Is.EqualTo(entity.Settings.PrivateMessagesAreEnabled));
            Assert.That(player.Settings.PrivateMessagesInterceptionIsEnabled, Is.EqualTo(entity.Settings.PrivateMessagesInterceptionIsEnabled));
            Assert.That(player.Settings.AutomaticHotbarRefillingIsEnabled, Is.EqualTo(entity.Settings.AutomaticHotbarRefillingIsEnabled));
            Assert.That(player.Settings.KeepInventoryIsEnabled, Is.EqualTo(entity.Settings.KeepInventoryIsEnabled));
            Assert.That(player.Settings.KeepExperienceIsEnabled, Is.EqualTo(entity.Settings.KeepExperienceIsEnabled));
            Assert.That(player.Settings.AutomaticToolSelectionIsEnabled, Is.EqualTo(entity.Settings.AutomaticToolSelectionIsEnabled));
            Assert.That(player.Settings.Localisation, Is.EqualTo(Localisation.English));
            Assert.That(player.Settings.SkinUrl, Is.EqualTo(entity.Settings.SkinUrl));
        }

        [Test]
        public void GivenAValidIdentifier_WhenGettingAPlayer_ThenThePlayerIsReturned()
        {
            PlayerDataObject entity = BuildPlayerDataObject();

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
            PlayerDataObject entity = BuildPlayerDataObject();

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
            PlayerDataObject entity = BuildPlayerDataObject();

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
            PlayerDataObject capturedEntity = null;
            PlayerDataObject original = BuildPlayerDataObject();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            PatchPlayerRequest request = new()
            {
                PlayerIdentifier = "IlarionPintilie",
                Username = "NewUsername",
                OnlineUUID = "11111111-0000-0000-0000-000000000000",
                Password = "NewPass",
                IpAddress = "10.0.0.1",
                DiscordId = "999",
                EmailAddress = "new@nucilandia.ro",
                LastSleptDT = "2026-01-01T00:00:00.0000000+00:00",
                LastDeathDT = "2026-06-01T00:00:00.0000000+00:00",
                LastDeathLocation = new() { World = "world_nether", X = 1.0f, Y = 2.0f, Z = 3.0f, Pitch = 4.0f, Yaw = 5.0f },
                BackLocation = new() { World = "world", X = 6.0f, Y = 7.0f, Z = 8.0f, Pitch = 9.0f, Yaw = 10.0f },
                LogoutLocation = new() { World = "world_the_end", X = -10.0f, Y = 80.0f, Z = 41.0f, Pitch = 50.0f, Yaw = 60.0f },
                Settings = new()
                {
                    AutomaticHotbarRefillingIsEnabled = true,
                    AutomaticSaplingReplantingIsEnabled = false,
                    AutomaticToolSelectionIsEnabled = false,
                    KeepExperienceIsEnabled = true,
                    KeepInventoryIsEnabled = false,
                    PrivateMessagesAreEnabled = true,
                    PrivateMessagesInterceptionIsEnabled = false,
                    Localisation = "romanian",
                    SkinUrl = "new-skin.nucilandia.ro"
                }
            };

            playerService.Update(request);

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
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
            Assert.That(capturedEntity.BackLocation, Is.Not.Null);
            Assert.That(capturedEntity.BackLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BackLocation.X, Is.EqualTo(6.0f));
            Assert.That(capturedEntity.BackLocation.Y, Is.EqualTo(7.0f));
            Assert.That(capturedEntity.BackLocation.Z, Is.EqualTo(8.0f));
            Assert.That(capturedEntity.BackLocation.Pitch, Is.EqualTo(9.0f));
            Assert.That(capturedEntity.BackLocation.Yaw, Is.EqualTo(10.0f));
            Assert.That(capturedEntity.LogoutLocation, Is.Not.Null);
            Assert.That(capturedEntity.LogoutLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.LogoutLocation.X, Is.EqualTo(-10.0f));
            Assert.That(capturedEntity.LogoutLocation.Y, Is.EqualTo(80.0f));
            Assert.That(capturedEntity.LogoutLocation.Z, Is.EqualTo(41.0f));
            Assert.That(capturedEntity.LogoutLocation.Pitch, Is.EqualTo(50.0f));
            Assert.That(capturedEntity.LogoutLocation.Yaw, Is.EqualTo(60.0f));
            Assert.That(capturedEntity.Settings, Is.Not.Null);
            Assert.That(capturedEntity.Settings.Localisation, Is.EqualTo("romanian"));
            Assert.That(capturedEntity.Settings.SkinUrl, Is.EqualTo("new-skin.nucilandia.ro"));
            Assert.That(capturedEntity.Settings.AutomaticHotbarRefillingIsEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.AutomaticSaplingReplantingIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.AutomaticToolSelectionIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.KeepExperienceIsEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.KeepInventoryIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.PrivateMessagesAreEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.PrivateMessagesInterceptionIsEnabled, Is.EqualTo(false));
        }

        [Test]
        public void GivenPatchRequestWithUsernameAndOnlineUuid_WhenUpdatingAPlayer_ThenIdentityFieldsRemainUnchanged()
        {
            PlayerDataObject capturedEntity = null;
            PlayerDataObject original = BuildPlayerDataObject();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerIdentifier = "IlarionPintilie",
                Username = "AnotherUsername",
                OnlineUUID = "99999999-0000-0000-0000-000000000000",
                Password = "NucilandiaPass2"
            });

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass2"));
        }

        [Test]
        public void GivenARequestWithNullFields_WhenUpdatingAPlayer_ThenExistingValuesArePreserved()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest { PlayerIdentifier = "IlarionPintilie" });

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
            Assert.That(capturedEntity.LogoutLocation, Is.EqualTo(original.LogoutLocation));
            Assert.That(capturedEntity.Settings.SkinUrl, Is.EqualTo(original.Settings.SkinUrl));
            Assert.That(capturedEntity.Settings, Is.EqualTo(original.Settings));
        }

        [Test]
        public void GivenARequestWithPartialCoordinates_WhenUpdatingAPlayer_ThenTheCoordinatesObjectIsReplaced()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.LastDeathLocation = new()
            {
                World = "world",
                X = 100.5f,
                Y = 70.0f,
                Z = -25.25f,
                Pitch = 45.0f,
                Yaw = 90.0f
            };
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerIdentifier = "IlarionPintilie",
                LastDeathLocation = new() { Y = 64.0f }
            });

            Assert.That(capturedEntity.LastDeathLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastDeathLocation.World, Is.Null);
            Assert.That(capturedEntity.LastDeathLocation.X, Is.EqualTo(0.0f));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(64.0f));
            Assert.That(capturedEntity.LastDeathLocation.Z, Is.EqualTo(0.0f));
            Assert.That(capturedEntity.LastDeathLocation.Pitch, Is.EqualTo(0.0f));
            Assert.That(capturedEntity.LastDeathLocation.Yaw, Is.EqualTo(179.9f));
        }

        [Test]
        public void GivenARequestWithPartialSettings_WhenUpdatingAPlayer_ThenOnlyProvidedSettingsFieldsAreUpdated()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerIdentifier = "IlarionPintilie",
                Settings = new()
                {
                    KeepInventoryIsEnabled = false,
                    PrivateMessagesAreEnabled = true,
                    AutomaticHotbarRefillingIsEnabled = null,
                    AutomaticSaplingReplantingIsEnabled = null,
                    AutomaticToolSelectionIsEnabled = null,
                    KeepExperienceIsEnabled = null,
                    PrivateMessagesInterceptionIsEnabled = null
                }
            });

            Assert.That(capturedEntity.Settings, Is.Not.Null);
            Assert.That(capturedEntity.Settings.AutomaticSaplingReplantingIsEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.PrivateMessagesAreEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.PrivateMessagesInterceptionIsEnabled, Is.EqualTo(true));
            Assert.That(capturedEntity.Settings.AutomaticHotbarRefillingIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.KeepInventoryIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.KeepExperienceIsEnabled, Is.EqualTo(false));
            Assert.That(capturedEntity.Settings.AutomaticToolSelectionIsEnabled, Is.EqualTo(true));
        }

        [Test]
        public void GivenARequestWithLastDeathLocationWhenEntityHasNone_WhenUpdatingAPlayer_ThenLastDeathLocationIsCreated()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.LastDeathLocation = null;
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerIdentifier = "IlarionPintilie",
                LastDeathLocation = new() { World = "world_the_end", X = 0f, Y = 64f, Z = 0f }
            });

            Assert.That(capturedEntity.LastDeathLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastDeathLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(64f));
        }

        [Test]
        public void GivenARequestWithBackLocationWhenEntityHasNone_WhenUpdatingAPlayer_ThenBackLocationIsCreated()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.BackLocation = null;
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerIdentifier = "IlarionPintilie",
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
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            playerService.Update(new PatchPlayerRequest { PlayerIdentifier = "IlarionPintilie" });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAPlayer_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            playerService.Update(new PatchPlayerRequest { PlayerIdentifier = "IlarionPintilie" });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAPlayer_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { PlayerIdentifier = "NonExistentPlayer" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAUsernameSelector_WhenPatchingAPlayer_ThenThePlayerIsUpdated()
        {
            PlayerDataObject capturedEntity = null;
            PlayerDataObject original = BuildPlayerDataObject();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerUsername = "IlarionPintilie",
                Password = "NucilandiaPass2"
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass2"));
        }

        [Test]
        public void GivenAnOfflineUUIDSelector_WhenPatchingAPlayer_ThenThePlayerIsUpdated()
        {
            PlayerDataObject capturedEntity = null;
            PlayerDataObject original = BuildPlayerDataObject();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerOfflineUUID = "61300000-0000-3000-8000-000000000000",
                IpAddress = "10.8.0.42"
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.IpAddress, Is.EqualTo("10.8.0.42"));
        }

        [Test]
        public void GivenAnOnlineUUIDSelector_WhenPatchingAPlayer_ThenThePlayerIsUpdated()
        {
            PlayerDataObject capturedEntity = null;
            PlayerDataObject original = BuildPlayerDataObject();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                PlayerOnlineUUID = "87300000-0000-0000-0000-000000000000",
                EmailAddress = "solaire@astora.com"
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.EmailAddress, Is.EqualTo("solaire@astora.com"));
        }

        [Test]
        public void GivenMultipleSelectors_WhenPatchingAPlayer_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Update(new PatchPlayerRequest
                {
                    PlayerIdentifier = "IlarionPintilie",
                    PlayerUsername = "IlarionPintilie"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenNoSelectors_WhenPatchingAPlayer_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { Username = "NewUsername" }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenANonExistentSelector_WhenPatchingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { PlayerIdentifier = "non-existent-player" }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        private static RegisterPlayerRequest BuildRegisterPlayerRequest() => new()
        {
            Username = "IlarionPintilie",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            Password = "NucilandiaPass1",
            IpAddress = "192.168.1.1",
        };

        private static PlayerDataObject BuildPlayerDataObject() => new()
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
            LogoutLocation = new() { World = "world", X = -13.0f, Y = 75.0f, Z = 22.5f, Pitch = 15.0f, Yaw = 40.0f },
            Settings = new()
            {
                AutomaticSaplingReplantingIsEnabled = true,
                PrivateMessagesAreEnabled = false,
                PrivateMessagesInterceptionIsEnabled = true,
                AutomaticHotbarRefillingIsEnabled = false,
                KeepInventoryIsEnabled = true,
                KeepExperienceIsEnabled = false,
                AutomaticToolSelectionIsEnabled = true,
                Localisation = "english",
                SkinUrl = "test.nucilandia.ro"
            }
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
