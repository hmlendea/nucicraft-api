using System;
using System.Collections.Generic;
using System.Linq;

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
    public sealed class WorldServiceTests
    {
        private Mock<IFileRepository<WorldDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private WorldService worldService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<WorldDataObject>>();
            loggerMock = new Mock<ILogger>();
            worldService = new WorldService(repositoryMock.Object, loggerMock.Object);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAWorld_ThenTheWorldIsPersisted()
        {
            WorldDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<WorldDataObject>()))
                .Callback<WorldDataObject>(entity => capturedEntity = entity);

            AddWorldRequest request = new()
            {
                Identifier = "main",
                Name = new() { English = "Main World", Romanian = "Lumea Principala" }
            };

            worldService.Add(request);

            Assert.That(capturedEntity.Id, Is.EqualTo("main"));
            Assert.That(capturedEntity.Name.English, Is.EqualTo("Main World"));
            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("Lumea Principala"));
            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAWorld_ThenSaveChangesIsInvoked()
        {
            AddWorldRequest request = new()
            {
                Identifier = "main"
            };

            worldService.Add(request);

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenAddingAWorld_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<WorldDataObject>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => worldService.Add(new AddWorldRequest { Identifier = "main" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenANullRequest_WhenAddingAWorld_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => worldService.Add(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenAnExistingWorld_WhenGettingAWorld_ThenTheWorldIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(BuildWorldDataObject());

            World world = worldService.GetWorld("main");

            Assert.That(world.Identifier, Is.EqualTo("main"));
            Assert.That(world.Name.English, Is.EqualTo("Main World"));
            Assert.That(world.Name.Romanian, Is.EqualTo("Lumea Principala"));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAWorld_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => worldService.GetWorld("main"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenWorldsInTheRepository_WhenGettingAllWorlds_ThenAllWorldsAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([
                    BuildWorldDataObject(),
                    new WorldDataObject
                    {
                        Id = "nether",
                        Name = new LocalisedStringDataObject { English = "Nether" }
                    }
                ]);

            List<World> worlds = worldService.GetAllWorlds().ToList();

            Assert.That(worlds, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAllWorlds_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => worldService.GetAllWorlds(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenARequestWithPartialLocalisation_WhenUpdatingAWorld_ThenOnlyProvidedLocalisationsAreUpdated()
        {
            WorldDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(BuildWorldDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<WorldDataObject>()))
                .Callback<WorldDataObject>(entity => capturedEntity = entity);

            PatchWorldRequest request = new()
            {
                Identifier = "main",
                Name = new() { Romanian = "Lumea Centrala" }
            };

            worldService.Update(request);

            Assert.That(capturedEntity.Name.English, Is.EqualTo("Main World"));
            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("Lumea Centrala"));
            Assert.That(capturedEntity.Name.German, Is.EqualTo("Hauptwelt"));
        }

        [Test]
        public void GivenANullExistingName_WhenUpdatingAWorld_ThenTheIncomingNameIsApplied()
        {
            WorldDataObject worldDataObject = BuildWorldDataObject();
            worldDataObject.Name = null;
            WorldDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(worldDataObject);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<WorldDataObject>()))
                .Callback<WorldDataObject>(entity => capturedEntity = entity);

            worldService.Update(new PatchWorldRequest
            {
                Identifier = "main",
                Name = new() { English = "Main Realm" }
            });

            Assert.That(capturedEntity.Name.English, Is.EqualTo("Main Realm"));
        }

        [Test]
        public void GivenARequestWithNoIdentifier_WhenUpdatingAWorld_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => worldService.Update(new PatchWorldRequest
                {
                    Name = new LocalisedStringDataObject { English = "Main Realm" }
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAWorld_ThenUpdatedDTIsStamped()
        {
            WorldDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(BuildWorldDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<WorldDataObject>()))
                .Callback<WorldDataObject>(entity => capturedEntity = entity);

            worldService.Update(new PatchWorldRequest
            {
                Identifier = "main"
            });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAWorld_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(BuildWorldDataObject());

            worldService.Update(new PatchWorldRequest
            {
                Identifier = "main"
            });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAWorld_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => worldService.Update(new PatchWorldRequest { Identifier = "main" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenANonExistentIdentifier_WhenUpdatingAWorld_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("non-existent-world"))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => worldService.Update(new PatchWorldRequest { Identifier = "non-existent-world" }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenANullRequest_WhenUpdatingAWorld_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => worldService.Update(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        private static WorldDataObject BuildWorldDataObject() => new()
        {
            Id = "main",
            Name = new LocalisedStringDataObject
            {
                English = "Main World",
                German = "Hauptwelt",
                Romanian = "Lumea Principala"
            }
        };
    }
}
