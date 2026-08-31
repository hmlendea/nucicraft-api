using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

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
    public sealed class PlayerServiceTests
    {
        private static readonly JsonSerializerOptions playerSettingsJsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        private static string OfflineUuidVersionThreeRfc4122Pattern
            => "^[0-9a-f]{8}-[0-9a-f]{4}-3[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$";

        private Mock<IFileRepository<PlayerDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private PlayerService playerService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<PlayerDataObject>>();
            loggerMock = new Mock<ILogger>();
            playerService = new PlayerService(repositoryMock.Object, loggerMock.Object);
        }

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
            Assert.That(capturedEntity.DisplayName, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(capturedEntity.Gender, Is.EqualTo("female"));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo("87300000-0000-0000-0000-000000000000"));
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass1"));
            Assert.That(capturedEntity.LastIpAddress, Is.EqualTo("192.168.1.1"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://test.nucilandia.ro"));
            Assert.That(capturedEntity.IsBanned);
            Assert.That(capturedEntity.BannedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.IsMuted);
            Assert.That(capturedEntity.MutedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLoginDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLogoutDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLogoutLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastLogoutLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.LastLogoutLocation.X, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.LastLogoutLocation.Y, Is.EqualTo(64.0f));
            Assert.That(capturedEntity.LastLogoutLocation.Z, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.LastSleptLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastSleptLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.LastSleptLocation.X, Is.EqualTo(5.0f));
            Assert.That(capturedEntity.LastSleptLocation.Y, Is.EqualTo(70.0f));
            Assert.That(capturedEntity.LastSleptLocation.Z, Is.EqualTo(-3.0f));
            Assert.That(capturedEntity.BedLocation, Is.Not.Null);
            Assert.That(capturedEntity.BedLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BedLocation.X, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.BedLocation.Y, Is.EqualTo(64.0f));
            Assert.That(capturedEntity.BedLocation.Z, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.BackDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.Settings, Is.Not.Null);
            Assert.That(capturedEntity.Settings.Localisation, Is.EqualTo("romanian"));
            Assert.That(capturedEntity.Settings.SkinUrl, Is.Null);
            Assert.That(capturedEntity.Settings.TeleportationRequestsAreEnabled);
        }

        [Test]
        public void GivenARequestWithoutOptionalState_WhenRegistering_ThenOptionalStateUsesDefaults()
        {
            RegisterPlayerRequest request = new()
            {
                Username = "IlarionPintilie",
                CreatedDT = "2012-09-05T00:00:00.0000000+00:00"
            };
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity.DisplayName, Is.Null);
            Assert.That(capturedEntity.Gender, Is.EqualTo("other"));
            Assert.That(capturedEntity.WikiUrl, Is.Null);
            Assert.That(capturedEntity.IsBanned, Is.False);
            Assert.That(capturedEntity.BannedDT, Is.Null);
            Assert.That(capturedEntity.IsMuted, Is.False);
            Assert.That(capturedEntity.MutedDT, Is.Null);
            Assert.That(capturedEntity.LastLoginDT, Is.Null);
            Assert.That(capturedEntity.LastLogoutDT, Is.Null);
            Assert.That(capturedEntity.LastLogoutLocation, Is.Null);
            Assert.That(capturedEntity.LastSleptLocation, Is.Null);
            Assert.That(capturedEntity.BedLocation, Is.Null);
            Assert.That(capturedEntity.BackDT, Is.Null);
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
        public void GivenAUsername_WhenRegistering_ThenTheOfflineUuidUsesVersionThreeAndTheRfc4122Variant()
        {
            PlayerDataObject capturedEntity = null;
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Register(BuildRegisterPlayerRequest());

            Assert.That(
                capturedEntity.OfflineUUID,
                Does.Match(OfflineUuidVersionThreeRfc4122Pattern));
        }

        [TestCase("AndreiMirea", "7abb798a-c25e-3a21-8b28-ca2aad2881bd")]
        [TestCase("beepbeep", "ff56aee7-976c-3e7b-8f28-0b33ac2148fd")]
        [TestCase("Blitzkrieg94", "a01d8afa-3752-3a20-98a2-74bab8778e41")]
        [TestCase("Ionut22", "b7029140-b155-3deb-8f48-439b16ebcd58")]
        [TestCase("Mary", "ce43e0a4-598c-3b41-aa45-54ac28944dae")]
        [TestCase("mibu", "2fd7404d-987c-3665-b1fe-3972615acc9c")]
        [TestCase("nnivrim", "be1be236-9710-3639-b3e1-81b90cb46688")]
        [TestCase("qAviis", "84adb7b1-f26f-3702-abd8-b3a097975ac2")]
        public void GivenAMinecraftUsername_WhenRegistering_ThenTheExpectedOfflineUuidIsCalculated(
            string username,
            string expectedOfflineUuid)
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest(username);
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Register(request);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.OfflineUUID, Is.EqualTo(expectedOfflineUuid));
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
        public void GivenARequestWithInvalidCreatedDT_WhenRegistering_ThenAnArgumentExceptionIsThrown()
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            request.CreatedDT = "invalid-timestamp";

            Assert.That(
                () => playerService.Register(request),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        [TestCase(" 2012-09-05T00:00:00.0000000+00:00")]
        [TestCase("2012-09-05T00:00:00.0000000+00:00 ")]
        [TestCase("2012-09-05T00:00:00.000000+00:00")]
        [TestCase("2012-09-05T00:00:00.00000000+00:00")]
        [TestCase("2012-09-05t00:00:00.0000000+00:00")]
        [TestCase("2012-09-05T00:00:00,0000000+00:00")]
        public void GivenANonConformingCreatedTimestamp_WhenRegistering_ThenAnArgumentExceptionIsThrown(
            string createdTimestamp)
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            request.CreatedDT = createdTimestamp;

            Assert.That(
                () => playerService.Register(request),
                Throws.TypeOf<ArgumentException>()
                    .With.Property(nameof(ArgumentException.ParamName))
                    .EqualTo(nameof(RegisterPlayerRequest.CreatedDT)));
            repositoryMock.Verify(
                repository => repository.Add(It.IsAny<PlayerDataObject>()),
                Times.Never);
        }

        [TestCase(nameof(RegisterPlayerRequest.BannedDT))]
        [TestCase(nameof(RegisterPlayerRequest.MutedDT))]
        [TestCase(nameof(RegisterPlayerRequest.LastLoginDT))]
        [TestCase(nameof(RegisterPlayerRequest.LastLogoutDT))]
        [TestCase(nameof(RegisterPlayerRequest.BackDT))]
        public void GivenANonConformingOptionalTimestamp_WhenRegistering_ThenAnArgumentExceptionIdentifiesTheTimestamp(
            string timestampPropertyName)
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            PropertyInfo timestampProperty = typeof(RegisterPlayerRequest)
                .GetProperty(timestampPropertyName);
            timestampProperty.SetValue(request, "invalid-timestamp");

            Assert.That(
                () => playerService.Register(request),
                Throws.TypeOf<ArgumentException>()
                    .With.Property(nameof(ArgumentException.ParamName))
                    .EqualTo(timestampPropertyName));
            repositoryMock.Verify(
                repository => repository.Add(It.IsAny<PlayerDataObject>()),
                Times.Never);
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

        [Test]
        public void GivenANullRequest_WhenRegistering_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Register(null),
                Throws.TypeOf<ArgumentNullException>());
        }

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
            Assert.That(player.DisplayName, Is.EqualTo(entity.DisplayName));
            Assert.That(player.OfflineUUID, Is.EqualTo(entity.OfflineUUID));
            Assert.That(player.OnlineUUID, Is.EqualTo(entity.OnlineUUID));
            Assert.That(player.Password, Is.EqualTo(entity.Password));
            Assert.That(player.CreatedDT, Is.EqualTo(DateTimeOffset.Parse(entity.CreatedDT)));
            Assert.That(player.UpdatedDT, Is.Null);
            Assert.That(player.LastIpAddress, Is.EqualTo(entity.LastIpAddress));
            Assert.That(player.DiscordId, Is.EqualTo(entity.DiscordId));
            Assert.That(player.EmailAddress, Is.EqualTo(entity.EmailAddress));
            Assert.That(player.WikiUrl, Is.EqualTo(entity.WikiUrl));
            Assert.That(player.IsBanned, Is.EqualTo(entity.IsBanned));
            Assert.That(player.BannedDT, Is.EqualTo(DateTimeOffset.Parse(entity.BannedDT)));
            Assert.That(player.IsMuted, Is.EqualTo(entity.IsMuted));
            Assert.That(player.MutedDT, Is.EqualTo(DateTimeOffset.Parse(entity.MutedDT)));
            Assert.That(player.LastLoginDT, Is.EqualTo(DateTimeOffset.Parse(entity.LastLoginDT)));
            Assert.That(player.LastLogoutDT, Is.EqualTo(DateTimeOffset.Parse(entity.LastLogoutDT)));
            Assert.That(player.LastLogoutLocation, Is.Not.Null);
            Assert.That(player.LastLogoutLocation.World, Is.EqualTo(entity.LastLogoutLocation.World));
            Assert.That(player.LastLogoutLocation.X, Is.EqualTo(entity.LastLogoutLocation.X));
            Assert.That(player.LastLogoutLocation.Y, Is.EqualTo(entity.LastLogoutLocation.Y));
            Assert.That(player.LastLogoutLocation.Z, Is.EqualTo(entity.LastLogoutLocation.Z));
            Assert.That(player.LastLogoutLocation.Pitch, Is.EqualTo(entity.LastLogoutLocation.Pitch));
            Assert.That(player.LastLogoutLocation.Yaw, Is.EqualTo(entity.LastLogoutLocation.Yaw));
            Assert.That(player.LastSleptDT, Is.EqualTo(DateTimeOffset.Parse(entity.LastSleptDT)));
            Assert.That(player.BedLocation, Is.Not.Null);
            Assert.That(player.BedLocation.World, Is.EqualTo(entity.BedLocation.World));
            Assert.That(player.BedLocation.X, Is.EqualTo(entity.BedLocation.X));
            Assert.That(player.BedLocation.Y, Is.EqualTo(entity.BedLocation.Y));
            Assert.That(player.BedLocation.Z, Is.EqualTo(entity.BedLocation.Z));
            Assert.That(player.BedLocation.Pitch, Is.EqualTo(entity.BedLocation.Pitch));
            Assert.That(player.BedLocation.Yaw, Is.EqualTo(entity.BedLocation.Yaw));
            Assert.That(player.LastDeathDT, Is.Null);
            Assert.That(player.LastDeathLocation, Is.Null);
            Assert.That(player.BackDT, Is.EqualTo(DateTimeOffset.Parse(entity.BackDT)));
            Assert.That(player.BackLocation, Is.Not.Null);
            Assert.That(player.BackLocation.World, Is.EqualTo(entity.BackLocation.World));
            Assert.That(player.BackLocation.X, Is.EqualTo(entity.BackLocation.X));
            Assert.That(player.BackLocation.Y, Is.EqualTo(entity.BackLocation.Y));
            Assert.That(player.BackLocation.Z, Is.EqualTo(entity.BackLocation.Z));
            Assert.That(player.BackLocation.Pitch, Is.EqualTo(entity.BackLocation.Pitch));
            Assert.That(player.BackLocation.Yaw, Is.EqualTo(entity.BackLocation.Yaw));
            Assert.That(player.Settings, Is.Not.Null);
            Assert.That(player.Settings.AutomaticSaplingReplantingIsEnabled, Is.EqualTo(entity.Settings.AutomaticSaplingReplantingIsEnabled));
            Assert.That(player.Settings.PrivateMessagesAreEnabled, Is.EqualTo(entity.Settings.PrivateMessagesAreEnabled));
            Assert.That(player.Settings.PrivateMessagesInterceptionIsEnabled, Is.EqualTo(entity.Settings.PrivateMessagesInterceptionIsEnabled));
            Assert.That(player.Settings.AutomaticHotbarRefillingIsEnabled, Is.EqualTo(entity.Settings.AutomaticHotbarRefillingIsEnabled));
            Assert.That(player.Settings.KeepInventoryIsEnabled, Is.EqualTo(entity.Settings.KeepInventoryIsEnabled));
            Assert.That(player.Settings.KeepExperienceIsEnabled, Is.EqualTo(entity.Settings.KeepExperienceIsEnabled));
            Assert.That(player.Settings.AutomaticToolSelectionIsEnabled, Is.EqualTo(entity.Settings.AutomaticToolSelectionIsEnabled));
            Assert.That(player.Settings.TeleportationRequestsAreEnabled, Is.EqualTo(entity.Settings.TeleportationRequestsAreEnabled));
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
        public void GivenNoMatchingOnlineUUID_WhenGettingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            Assert.That(
                () => playerService.Get(new GetPlayerRequest
                {
                    OnlineUUID = "61300000-8730-3000-8000-000000000000"
                }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenNoMatchingIdentifier_WhenGettingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            Assert.That(
                () => playerService.Get(new GetPlayerRequest
                {
                    Identifier = "61300000-8730-3000-8000-000000000000"
                }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenNoMatchingOfflineUUID_WhenGettingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            Assert.That(
                () => playerService.Get(new GetPlayerRequest
                {
                    OfflineUUID = "87300000-6130-3000-8000-000000000000"
                }),
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

        [Test]
        public void GivenANullRequest_WhenGettingAPlayer_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Get(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenPlayersInTheRepository_WhenGettingAllPlayers_ThenAllPlayersAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            IEnumerable<Player> players = playerService.GetAll();

            Assert.That(players, Has.Exactly(1).Items);
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAllPlayers_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.GetAll(),
                Throws.TypeOf<InvalidOperationException>());
        }

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
                Identifier = "IlarionPintilie",
                DisplayName = "Ilarion Pintilie",
                Gender = "male",
                Password = "NewPass",
                LastIpAddress = "10.0.0.1",
                DiscordId = "999",
                EmailAddress = "new@nucilandia.ro",
                WikiUrl = "https://dummy-url.ro",
                IsBanned = false,
                BannedDT = "2026-08-13T00:00:00.0000000+00:00",
                IsMuted = true,
                MutedDT = "2026-08-13T00:00:00.0000000+00:00",
                LastLoginDT = "2026-08-13T00:00:00.0000000+00:00",
                LastLogoutDT = "2026-08-13T00:00:00.0000000+00:00",
                LastLogoutLocation = new() { World = "world_the_end", X = -10.0f, Y = 80.0f, Z = 41.0f, Pitch = 50.0f, Yaw = 60.0f },
                LastSleptDT = "2026-01-01T00:00:00.0000000+00:00",
                LastSleptLocation = new() { World = "world", X = 6.0f, Y = 70.0f, Z = 8.0f, Pitch = 3.0f, Yaw = 4.0f },
                BedLocation = new() { World = "world", X = 6.13f, Y = 64.0f, Z = 8.73f, Pitch = 3.14f, Yaw = 42.0f },
                LastDeathDT = "2026-06-01T00:00:00.0000000+00:00",
                LastDeathLocation = new() { World = "world_nether", X = 1.0f, Y = 2.0f, Z = 3.0f, Pitch = 4.0f, Yaw = 5.0f },
                BackDT = "2026-08-13T00:00:00.0000000+00:00",
                BackLocation = new() { World = "world", X = 6.0f, Y = 7.0f, Z = 8.0f, Pitch = 9.0f, Yaw = 10.0f },
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
                    SkinUrl = "new-skin.nucilandia.ro",
                    TeleportationRequestsAreEnabled = false
                }
            };

            playerService.Update(request);

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.DisplayName, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(capturedEntity.Gender, Is.EqualTo("male"));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
            Assert.That(capturedEntity.Password, Is.EqualTo("NewPass"));
            Assert.That(capturedEntity.LastIpAddress, Is.EqualTo("10.0.0.1"));
            Assert.That(capturedEntity.DiscordId, Is.EqualTo("999"));
            Assert.That(capturedEntity.EmailAddress, Is.EqualTo("new@nucilandia.ro"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://dummy-url.ro"));
            Assert.That(capturedEntity.IsBanned, Is.False);
            Assert.That(capturedEntity.BannedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.IsMuted);
            Assert.That(capturedEntity.MutedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLoginDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLogoutDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastLogoutLocation, Is.Not.Null);
            Assert.That(capturedEntity.LastLogoutLocation.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.LastLogoutLocation.X, Is.EqualTo(-10.0f));
            Assert.That(capturedEntity.LastLogoutLocation.Y, Is.EqualTo(80.0f));
            Assert.That(capturedEntity.LastLogoutLocation.Z, Is.EqualTo(41.0f));
            Assert.That(capturedEntity.LastLogoutLocation.Pitch, Is.EqualTo(50.0f));
            Assert.That(capturedEntity.LastLogoutLocation.Yaw, Is.EqualTo(60.0f));
            Assert.That(capturedEntity.LastSleptDT, Is.EqualTo("2026-01-01T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastSleptLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.LastSleptLocation.X, Is.EqualTo(6.0f));
            Assert.That(capturedEntity.LastSleptLocation.Y, Is.EqualTo(70.0f));
            Assert.That(capturedEntity.LastSleptLocation.Z, Is.EqualTo(8.0f));
            Assert.That(capturedEntity.BedLocation, Is.Not.Null);
            Assert.That(capturedEntity.BedLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BedLocation.X, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.BedLocation.Y, Is.EqualTo(64.0f));
            Assert.That(capturedEntity.BedLocation.Z, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.BedLocation.Pitch, Is.EqualTo(3.14f));
            Assert.That(capturedEntity.BedLocation.Yaw, Is.EqualTo(42.0f));
            Assert.That(capturedEntity.LastDeathDT, Is.EqualTo("2026-06-01T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.LastDeathLocation.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.LastDeathLocation.X, Is.EqualTo(1.0f));
            Assert.That(capturedEntity.LastDeathLocation.Y, Is.EqualTo(2.0f));
            Assert.That(capturedEntity.LastDeathLocation.Z, Is.EqualTo(3.0f));
            Assert.That(capturedEntity.LastDeathLocation.Pitch, Is.EqualTo(4.0f));
            Assert.That(capturedEntity.LastDeathLocation.Yaw, Is.EqualTo(5.0f));
            Assert.That(capturedEntity.BackDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(capturedEntity.BackLocation, Is.Not.Null);
            Assert.That(capturedEntity.BackLocation.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.BackLocation.X, Is.EqualTo(6.0f));
            Assert.That(capturedEntity.BackLocation.Y, Is.EqualTo(7.0f));
            Assert.That(capturedEntity.BackLocation.Z, Is.EqualTo(8.0f));
            Assert.That(capturedEntity.BackLocation.Pitch, Is.EqualTo(9.0f));
            Assert.That(capturedEntity.BackLocation.Yaw, Is.EqualTo(10.0f));
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
            Assert.That(capturedEntity.Settings.TeleportationRequestsAreEnabled, Is.EqualTo(false));
        }

        [Test]
        public void GivenARequestWithoutIdentityPatchFields_WhenUpdatingAPlayer_ThenIdentityFieldsRemainUnchanged()
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
                Identifier = "IlarionPintilie",
                Password = "NucilandiaPass2"
            });

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.DisplayName, Is.EqualTo(original.DisplayName));
            Assert.That(capturedEntity.Gender, Is.EqualTo(original.Gender));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
            Assert.That(capturedEntity.Password, Is.EqualTo("NucilandiaPass2"));
        }

        [Test]
        public void GivenARequestWithoutOptionalFields_WhenUpdatingAPlayer_ThenExistingOptionalValuesArePreserved()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest { Identifier = "IlarionPintilie" });

            Assert.That(capturedEntity.Username, Is.EqualTo(original.Username));
            Assert.That(capturedEntity.DisplayName, Is.EqualTo(original.DisplayName));
            Assert.That(capturedEntity.OnlineUUID, Is.EqualTo(original.OnlineUUID));
            Assert.That(capturedEntity.Password, Is.EqualTo(original.Password));
            Assert.That(capturedEntity.LastIpAddress, Is.EqualTo(original.LastIpAddress));
            Assert.That(capturedEntity.DiscordId, Is.EqualTo(original.DiscordId));
            Assert.That(capturedEntity.EmailAddress, Is.EqualTo(original.EmailAddress));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo(original.WikiUrl));
            Assert.That(capturedEntity.IsBanned, Is.False);
            Assert.That(capturedEntity.BannedDT, Is.EqualTo(original.BannedDT));
            Assert.That(capturedEntity.IsMuted, Is.False);
            Assert.That(capturedEntity.MutedDT, Is.EqualTo(original.MutedDT));
            Assert.That(capturedEntity.LastLoginDT, Is.EqualTo(original.LastLoginDT));
            Assert.That(capturedEntity.LastLogoutDT, Is.EqualTo(original.LastLogoutDT));
            Assert.That(capturedEntity.LastLogoutLocation, Is.EqualTo(original.LastLogoutLocation));
            Assert.That(capturedEntity.LastSleptDT, Is.EqualTo(original.LastSleptDT));
            Assert.That(capturedEntity.LastSleptLocation, Is.EqualTo(original.LastSleptLocation));
            Assert.That(capturedEntity.BedLocation, Is.EqualTo(original.BedLocation));
            Assert.That(capturedEntity.LastDeathDT, Is.EqualTo(original.LastDeathDT));
            Assert.That(capturedEntity.LastDeathLocation, Is.EqualTo(original.LastDeathLocation));
            Assert.That(capturedEntity.BackDT, Is.EqualTo(original.BackDT));
            Assert.That(capturedEntity.BackLocation, Is.EqualTo(original.BackLocation));
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
                Identifier = "IlarionPintilie",
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
                Identifier = "IlarionPintilie",
                Settings = new()
                {
                    KeepInventoryIsEnabled = false,
                    PrivateMessagesAreEnabled = true
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
            Assert.That(capturedEntity.Settings.TeleportationRequestsAreEnabled, Is.EqualTo(false));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void GivenATeleportationRequestsPreference_WhenUpdatingAPlayer_ThenTheRequestedValueIsApplied(
            bool existingValue,
            bool requestedValue)
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.Settings.TeleportationRequestsAreEnabled = existingValue;
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                Identifier = "IlarionPintilie",
                Settings = new()
                {
                    TeleportationRequestsAreEnabled = requestedValue
                }
            });

            Assert.That(
                capturedEntity.Settings.TeleportationRequestsAreEnabled,
                Is.EqualTo(requestedValue));
        }

        [TestCase("{}", false, false)]
        [TestCase("{\"teleportationRequestsAreEnabled\":false}", true, false)]
        [TestCase("{\"teleportationRequestsAreEnabled\":true}", false, true)]
        public void GivenAJsonTeleportationPreference_WhenUpdatingAPlayer_ThenOnlyAProvidedValueIsApplied(
            string settingsJson,
            bool existingValue,
            bool expectedValue)
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.Settings.TeleportationRequestsAreEnabled = existingValue;
            PlayerDataObject capturedEntity = null;
            PatchPlayerSettingsRequest settings = JsonSerializer
                .Deserialize<PatchPlayerSettingsRequest>(
                    settingsJson,
                    playerSettingsJsonSerializerOptions);

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                Identifier = "IlarionPintilie",
                Settings = settings
            });

            Assert.That(
                capturedEntity.Settings.TeleportationRequestsAreEnabled,
                Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenNullExistingSettings_WhenUpdatingPlayerSettings_ThenSettingsAreCreated()
        {
            PlayerDataObject original = BuildPlayerDataObject();
            original.Settings = null;
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([original]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            playerService.Update(new PatchPlayerRequest
            {
                Identifier = "IlarionPintilie",
                Settings = new()
                {
                    SkinUrl = "test.nucilandia.ro",
                    TeleportationRequestsAreEnabled = false
                }
            });

            Assert.That(capturedEntity.Settings, Is.Not.Null);
            Assert.That(capturedEntity.Settings.SkinUrl, Is.EqualTo("test.nucilandia.ro"));
            Assert.That(capturedEntity.Settings.TeleportationRequestsAreEnabled, Is.EqualTo(false));
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
            PlayerDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<PlayerDataObject>()))
                .Callback<PlayerDataObject>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            playerService.Update(new PatchPlayerRequest { Identifier = "IlarionPintilie" });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAPlayer_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildPlayerDataObject()]);

            playerService.Update(new PatchPlayerRequest { Identifier = "IlarionPintilie" });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAPlayer_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { Identifier = "NonExistentPlayer" }),
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
                Username = "IlarionPintilie",
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
                OfflineUUID = "61300000-0000-3000-8000-000000000000",
                LastIpAddress = "10.8.0.42"
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.LastIpAddress, Is.EqualTo("10.8.0.42"));
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
                OnlineUUID = "87300000-0000-0000-0000-000000000000",
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
                    Identifier = "IlarionPintilie",
                    Username = "IlarionPintilie"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenNoSelectors_WhenPatchingAPlayer_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { Password = "NucilandiaPass2" }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenANonExistentSelector_WhenPatchingAPlayer_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => playerService.Update(new PatchPlayerRequest { Identifier = "non-existent-player" }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenANullRequest_WhenPatchingAPlayer_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => playerService.Update(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        private static RegisterPlayerRequest BuildRegisterPlayerRequest() => new()
        {
            Username = "IlarionPintilie",
            DisplayName = "Ilarion Pintilie",
            Gender = "female",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            Password = "NucilandiaPass1",
            LastIpAddress = "192.168.1.1",
            WikiUrl = "https://test.nucilandia.ro",
            IsBanned = true,
            BannedDT = "2026-08-13T00:00:00.0000000+00:00",
            IsMuted = true,
            MutedDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLoginDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutLocation = new() { World = "world_the_end", X = 6.13f, Y = 64.0f, Z = 8.73f, Pitch = 3.14f, Yaw = 42.0f },
            LastSleptLocation = new() { World = "world", X = 5.0f, Y = 70.0f, Z = -3.0f, Pitch = 0.0f, Yaw = 90.0f },
            BedLocation = new() { World = "world", X = 8.73f, Y = 64.0f, Z = 6.13f, Pitch = 42.0f, Yaw = 3.14f },
            BackDT = "2026-08-13T00:00:00.0000000+00:00",
        };

        private static RegisterPlayerRequest BuildRegisterPlayerRequest(string username)
        {
            RegisterPlayerRequest request = BuildRegisterPlayerRequest();
            request.Username = username;

            return request;
        }

        private static PlayerDataObject BuildPlayerDataObject() => new()
        {
            Id = "IlarionPintilie",
            Username = "IlarionPintilie",
            DisplayName = "Ilarion",
            Gender = "female",
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucilandiaPass1",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            UpdatedDT = null,
            LastIpAddress = "192.168.1.1",
            DiscordId = null,
            EmailAddress = "ilarion.pintilie@nucilandia.ro",
            WikiUrl = "https://test.nucilandia.ro",
            IsBanned = true,
            BannedDT = "2026-08-13T00:00:00.0000000+00:00",
            IsMuted = false,
            MutedDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLoginDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutLocation = new() { World = "world", X = -13.0f, Y = 75.0f, Z = 22.5f, Pitch = 15.0f, Yaw = 40.0f },
            LastSleptDT = "2012-09-05T00:00:00.0000000+00:00",
            LastSleptLocation = new() { World = "world", X = 5.0f, Y = 70.0f, Z = -3.0f, Pitch = 0.0f, Yaw = 90.0f },
            BedLocation = new() { World = "world", X = 6.13f, Y = 64.0f, Z = 8.73f, Pitch = 3.14f, Yaw = 42.0f },
            LastDeathDT = null,
            LastDeathLocation = null,
            BackDT = "2026-08-13T00:00:00.0000000+00:00",
            BackLocation = new() { World = "world", X = 100.5f, Y = 70.0f, Z = -25.25f, Pitch = 45.0f, Yaw = 90.0f },
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
                SkinUrl = "test.nucilandia.ro",
                TeleportationRequestsAreEnabled = false
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
