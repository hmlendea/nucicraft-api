using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class HomeServiceTests
    {
        private Mock<IFileRepository<HomeDataObject>> repositoryMock;
        private Mock<IPlayerService> playerServiceMock;
        private Mock<ILogger> loggerMock;
        private HomeService service;
        private List<HomeDataObject> homes;

        [SetUp]
        public void SetUp()
        {
            homes = [];
            repositoryMock = new Mock<IFileRepository<HomeDataObject>>();
            repositoryMock.Setup(repository => repository.GetAll()).Returns(() => homes);
            repositoryMock.Setup(repository => repository.Add(It.IsAny<HomeDataObject>()))
                .Callback<HomeDataObject>(home => homes.Add(home));
            repositoryMock.Setup(repository => repository.Get(It.IsAny<string>()))
                .Returns((string identifier) => homes.Single(home => string.Equals(home.Id, identifier)));
            repositoryMock.Setup(repository => repository.Update(It.IsAny<HomeDataObject>()))
                .Callback<HomeDataObject>(home =>
                    homes[homes.FindIndex(stored => string.Equals(stored.Id, home.Id))] = home);
            repositoryMock.Setup(repository => repository.Remove(It.IsAny<string>()))
                .Callback<string>(identifier =>
                    homes.RemoveAll(home => string.Equals(home.Id, identifier)));
            playerServiceMock = new Mock<IPlayerService>();
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(new Player { Identifier = "player-id" });
            loggerMock = new Mock<ILogger>();
            service = new HomeService(repositoryMock.Object, playerServiceMock.Object, loggerMock.Object);
        }

        [Test]
        public void GivenAPlayerIdentifier_WhenAddingAHome_ThenTheIdentifierAndTimestampAreGenerated()
        {
            DateTimeOffset earliestCreation = DateTimeOffset.UtcNow;
            Home home = service.Add(BuildRequest("Astora"));

            Assert.That(Guid.Parse(home.Identifier), Is.Not.EqualTo(Guid.Empty));
            Assert.That(home.CreatedDT, Is.InRange(earliestCreation, DateTimeOffset.UtcNow));
            Assert.That(home.UpdatedDT, Is.Null);
            Assert.That(home.Player, Is.EqualTo("player-id"));
            playerServiceMock.Verify(playerService => playerService.Get(It.Is<GetPlayerRequest>(request =>
                string.Equals(request.Identifier, "player-id") && request.Username == null)), Times.Once);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [TestCase("Astora")]
        [TestCase("ASTORA")]
        [TestCase(" astora ")]
        public void GivenAnExistingNameForThePlayer_WhenAddingAHome_ThenTheDuplicateIsRejected(string name)
        {
            service.Add(BuildRequest("Astora"));

            Assert.That(() => service.Add(BuildRequest(name)), Throws.ArgumentException);
            Assert.That(homes, Has.Count.EqualTo(1));
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenDifferentPlayers_WhenAddingIdenticallyNamedHomes_ThenBothArePersisted()
        {
            Home first = service.Add(BuildRequest("Astora"));
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(new Player { Identifier = "other-player-id" });
            Home second = service.Add(BuildRequest("Astora"));

            Assert.That(homes, Has.Count.EqualTo(2));
            Assert.That(first.Identifier, Is.Not.EqualTo(second.Identifier));
            Assert.That(first.Player, Is.Not.EqualTo(second.Player));
        }

        [Test]
        public void GivenAnExistingHome_WhenDeletingIt_ThenTheHomeIsRemovedAndChangesAreSaved()
        {
            Home home = service.Add(BuildRequest("Astora"));

            service.Delete(home.Identifier);

            Assert.That(homes, Is.Empty);
            repositoryMock.Verify(repository => repository.Remove(home.Identifier), Times.Once);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void GivenARepositoryException_WhenDeletingAHome_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Remove("missing-home-id"))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => service.Delete("missing-home-id"),
                Throws.TypeOf<KeyNotFoundException>());
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Never);
        }

        [Test]
        public void GivenConflictingLocalisations_WhenPatchingAHome_ThenStoredValuesRemainIntact()
        {
            service.Add(BuildRequest("Astora"));
            Home second = service.Add(BuildRequest("Anor Londo"));

            Assert.That(() => service.Update(new PatchHomeRequest
            {
                Identifier = second.Identifier,
                Name = new LocalisedString { Romanian = " ASTORA " }
            }), Throws.ArgumentException);
            Home unchanged = service.Get(second.Identifier);

            Assert.That(unchanged.Name.English, Is.EqualTo("Anor Londo"));
            Assert.That(unchanged.Name.Romanian, Is.Null);
            Assert.That(unchanged.UpdatedDT, Is.Null);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void GivenAPartialPatch_WhenUpdatingAHome_ThenLocalisationsMergeAndCreationIsPreserved()
        {
            Home original = service.Add(BuildRequest("Astora"));
            Home updated = service.Update(new PatchHomeRequest
            {
                Identifier = original.Identifier,
                Name = new LocalisedString { Romanian = "Anor Londo" }
            });

            Assert.That(updated.Name.English, Is.EqualTo("Astora"));
            Assert.That(updated.Name.Romanian, Is.EqualTo("Anor Londo"));
            Assert.That(updated.Identifier, Is.EqualTo(original.Identifier));
            Assert.That(updated.CreatedDT, Is.EqualTo(original.CreatedDT));
            Assert.That(updated.UpdatedDT, Is.Not.Null);
            Assert.That(updated.Location.World, Is.EqualTo(original.Location.World));
            Assert.That(service.Get("player-id", " anor londo ").Identifier, Is.EqualTo(original.Identifier));
        }

        [TestCase(nameof(LocalisedString.Default))]
        [TestCase(nameof(LocalisedString.Chinese))]
        [TestCase(nameof(LocalisedString.Dacian))]
        [TestCase(nameof(LocalisedString.English))]
        [TestCase(nameof(LocalisedString.French))]
        [TestCase(nameof(LocalisedString.German))]
        [TestCase(nameof(LocalisedString.Italian))]
        [TestCase(nameof(LocalisedString.Japanese))]
        [TestCase(nameof(LocalisedString.Latin))]
        [TestCase(nameof(LocalisedString.Nucian))]
        [TestCase(nameof(LocalisedString.Romanian))]
        public void GivenALocalisedName_WhenCreatingAndQueryingAHome_ThenEveryTranslationIsRecognised(
            string localisation)
        {
            AddHomeRequest request = BuildRequest("Astora");
            request.Name = new LocalisedString();
            PropertyInfo property = typeof(LocalisedString).GetProperty(localisation);
            property.SetValue(request.Name, "Astora");
            Home home = service.Add(request);

            Assert.That(service.Get("player-id", "ASTORA").Identifier, Is.EqualTo(home.Identifier));
            Assert.That(() => service.Add(BuildRequest("Astora")), Throws.ArgumentException);
        }

        [Test]
        public void GivenHomesForMultiplePlayers_WhenListingHomes_ThenPlayerFilteringIsExact()
        {
            service.Add(BuildRequest("Astora"));
            service.Add(BuildRequest("Anor Londo"));
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(new Player { Identifier = "other-player-id" });
            service.Add(BuildRequest("Astora"));

            Assert.That(service.GetAll(), Has.Exactly(3).Items);
            Assert.That(service.GetAllByPlayer("player-id"), Has.Exactly(2).Items);
            Assert.That(service.GetAllByPlayer("other-player-id"), Has.Exactly(1).Items);
            Assert.That(service.GetAllByPlayer("DummyUser"), Is.Empty);
            Assert.That(service.GetAllByPlayer("missing-player-id"), Is.Empty);
            Assert.That(service.Get("other-player-id", "Astora").Player, Is.EqualTo("other-player-id"));
            Assert.That(() => service.Get("other-player-id", "Anor Londo"), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenNoHomes_WhenListingHomes_ThenTheResultsAreEmpty()
        {
            Assert.That(service.GetAll(), Is.Empty);
            Assert.That(service.GetAllByPlayer("player-id"), Is.Empty);
            Assert.That(() => service.Get("player-id", "Astora"), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenANewPlayerAndLocation_WhenPatchingAHome_ThenTheSuppliedValuesArePersisted()
        {
            Home original = service.Add(BuildRequest("Astora"));
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(new Player { Identifier = "other-player-id" });
            Home updated = service.Update(new PatchHomeRequest
            {
                Identifier = original.Identifier,
                Player = "other-player-id",
                Location = new Coordinates { World = "nether", X = -42, Y = 96, Z = 873, Pitch = 16, Yaw = 32 }
            });

            Assert.That(updated.Player, Is.EqualTo("other-player-id"));
            Assert.That(updated.Location.World, Is.EqualTo("nether"));
            Assert.That(updated.Location.X, Is.EqualTo(-42));
            Assert.That(updated.Location.Y, Is.EqualTo(96));
            Assert.That(updated.Location.Z, Is.EqualTo(873));
            Assert.That(updated.Location.Pitch, Is.EqualTo(16));
            Assert.That(updated.Location.Yaw, Is.EqualTo(32));
            Assert.That(updated.CreatedDT, Is.EqualTo(original.CreatedDT));
            Assert.That(service.GetAllByPlayer("player-id"), Is.Empty);
            playerServiceMock.Verify(playerService => playerService.Get(It.Is<GetPlayerRequest>(request =>
                string.Equals(request.Identifier, "other-player-id") && request.Username == null)), Times.Once);
        }

        [Test]
        public void GivenAConflictingNameForTheDestinationPlayer_WhenTransferringAHome_ThenNothingChanges()
        {
            Home first = service.Add(BuildRequest("Astora"));
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(new Player { Identifier = "other-player-id" });
            service.Add(BuildRequest("Astora"));

            Assert.That(() => service.Update(new PatchHomeRequest
            {
                Identifier = first.Identifier,
                Player = "other-player-id"
            }), Throws.ArgumentException);
            Assert.That(service.Get(first.Identifier).Player, Is.EqualTo("player-id"));
            Assert.That(service.Get(first.Identifier).UpdatedDT, Is.Null);
        }

        [Test]
        public void GivenAnUnchangedName_WhenPatchingAHome_ThenItDoesNotConflictWithItself()
        {
            Home home = service.Add(BuildRequest("Astora"));
            Home updated = service.Update(new PatchHomeRequest
            {
                Identifier = home.Identifier,
                Name = new LocalisedString { English = "ASTORA" }
            });

            Assert.That(updated.Name.English, Is.EqualTo("ASTORA"));
            Assert.That(homes, Has.Count.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GivenNoUsableName_WhenCreatingOrPatchingAHome_ThenTheRequestIsRejected(string name)
        {
            Assert.That(() => service.Add(BuildRequest(name)), Throws.ArgumentException);
            Home original = service.Add(BuildRequest("Astora"));

            if (name is not null)
            {
                Assert.That(() => service.Update(new PatchHomeRequest
                {
                    Identifier = original.Identifier,
                    Name = new LocalisedString { English = name }
                }), Throws.ArgumentException);
            }

            Assert.That(service.Get(original.Identifier).Name.English, Is.EqualTo("Astora"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GivenAnInvalidSelector_WhenRetrievingOrUpdatingHomes_ThenItIsRejected(string selector)
        {
            Assert.That(() => service.Delete(selector), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.Get(selector), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.GetAllByPlayer(selector), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.Get(selector, "Astora"), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.Get("player-id", selector), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.Update(new PatchHomeRequest { Identifier = selector }),
                Throws.InstanceOf<ArgumentException>());
            AddHomeRequest request = BuildRequest("Astora");
            request.Player = selector;

            Assert.That(() => service.Add(request), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void GivenMissingRequiredObjects_WhenAddingOrPatchingAHome_ThenNothingIsPersisted()
        {
            Assert.That(() => service.Add(null), Throws.ArgumentNullException);
            Assert.That(() => service.Update(null), Throws.ArgumentNullException);
            AddHomeRequest request = BuildRequest("Astora");
            request.Name = null;

            Assert.That(() => service.Add(request), Throws.ArgumentNullException);
            request = BuildRequest("Astora");
            request.Location = null;

            Assert.That(() => service.Add(request), Throws.ArgumentNullException);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Never);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GivenAnInvalidWorld_WhenCreatingOrPatchingAHome_ThenNothingChanges(string world)
        {
            Home home = service.Add(BuildRequest("Astora"));
            AddHomeRequest request = BuildRequest("Anor Londo");
            request.Location.World = world;

            Assert.That(() => service.Add(request), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => service.Update(new PatchHomeRequest
            {
                Identifier = home.Identifier,
                Location = request.Location
            }), Throws.InstanceOf<ArgumentException>());
            Assert.That(service.Get(home.Identifier).Location.World, Is.EqualTo("world"));
        }

        [TestCase(nameof(Coordinates.X), float.NaN)]
        [TestCase(nameof(Coordinates.Y), float.PositiveInfinity)]
        [TestCase(nameof(Coordinates.Z), float.NegativeInfinity)]
        [TestCase(nameof(Coordinates.Pitch), float.NaN)]
        [TestCase(nameof(Coordinates.Yaw), float.PositiveInfinity)]
        public void GivenNonFiniteCoordinates_WhenAddingAHome_ThenTheyAreRejected(string component, float value)
        {
            AddHomeRequest request = BuildRequest("Astora");
            typeof(Coordinates).GetProperty(component).SetValue(request.Location, value);

            Assert.That(() => service.Add(request), Throws.ArgumentException);
        }

        [Test]
        public void GivenAnUnknownPlayer_WhenCreatingOrTransferringAHome_ThenNothingChanges()
        {
            Home home = service.Add(BuildRequest("Astora"));
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Throws(new KeyNotFoundException());

            Assert.That(() => service.Add(BuildRequest("Anor Londo")), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => service.Update(new PatchHomeRequest
            {
                Identifier = home.Identifier,
                Player = "missing-player-id"
            }), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(service.Get(home.Identifier).Player, Is.EqualTo("player-id"));
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenAnUnknownPlayer_WhenCreatingAHome_ThenTheAttemptedPlayerIdentifierIsLogged()
        {
            playerServiceMock.Setup(playerService => playerService.Get(It.IsAny<GetPlayerRequest>()))
                .Throws(new KeyNotFoundException());

            Assert.That(
                () => service.Add(BuildRequest("Astora")),
                Throws.TypeOf<KeyNotFoundException>());
            loggerMock.Verify(logger => logger.Error(
                It.Is<Operation>(operation => string.Equals(operation.Name, MyOperation.AddHome.Name)),
                It.Is<OperationStatus>(operationStatus =>
                    string.Equals(operationStatus.Name, OperationStatus.Failure.Name)),
                It.IsAny<KeyNotFoundException>(),
                It.Is<IEnumerable<LogInfo>>(logInfos => logInfos.Any(logInfo =>
                    string.Equals(logInfo.Key.Name, MyLogInfoKey.PlayerID.Name) &&
                    string.Equals(logInfo.Value, "player-id")))),
                Times.Once);
        }

        [Test]
        public async Task GivenConcurrentDuplicateRequests_WhenAddingHomes_ThenOnlyOneIsPersisted()
        {
            IEnumerable<Task<bool>> requests = Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
            {
                try
                {
                    service.Add(BuildRequest("Astora"));

                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }));
            bool[] results = await Task.WhenAll(requests);

            Assert.That(results.Count(isSuccessful => isSuccessful), Is.EqualTo(1));
            Assert.That(homes, Has.Count.EqualTo(1));
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        private static AddHomeRequest BuildRequest(string name) => new()
        {
            Player = "player-id",
            Name = new LocalisedString { English = name },
            Location = new Coordinates { World = "world", X = 42, Y = 64, Z = 613 }
        };
    }
}