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
                Name = new() { English = "Main World", Romanian = "Lumea Principala" },
                HasWebMap = true,
                SpawnPoint = new()
                {
                    World = "world",
                    X = 6.13f,
                    Y = 64f,
                    Z = 8.73f,
                    Pitch = 3.14f,
                    Yaw = 42f
                },
                Type = "nether"
            };

            worldService.Add(request);

            Assert.That(capturedEntity.Id, Is.EqualTo("main"));
            Assert.That(capturedEntity.Name.English, Is.EqualTo("Main World"));
            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("Lumea Principala"));
            Assert.That(capturedEntity.HasWebMap);
            Assert.That(capturedEntity.SpawnPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.SpawnPoint.X, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.SpawnPoint.Y, Is.EqualTo(64f));
            Assert.That(capturedEntity.SpawnPoint.Z, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.SpawnPoint.Pitch, Is.EqualTo(3.14f));
            Assert.That(capturedEntity.SpawnPoint.Yaw, Is.EqualTo(42f));
            Assert.That(capturedEntity.Type, Is.EqualTo("nether"));
            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenARequestWithoutOptionalProperties_WhenAddingAWorld_ThenDefaultsArePersisted()
        {
            WorldDataObject capturedEntity = null;
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<WorldDataObject>()))
                .Callback<WorldDataObject>(entity => capturedEntity = entity);

            worldService.Add(new AddWorldRequest { Identifier = "main" });

            Assert.That(capturedEntity.HasWebMap, Is.False);
            Assert.That(capturedEntity.SpawnPoint, Is.Null);
            Assert.That(capturedEntity.Type, Is.EqualTo("overworld"));
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
            Assert.That(world.HasWebMap);
            Assert.That(world.SpawnPoint.World, Is.EqualTo("world"));
            Assert.That(world.SpawnPoint.X, Is.EqualTo(6.13f));
            Assert.That(world.SpawnPoint.Y, Is.EqualTo(64f));
            Assert.That(world.SpawnPoint.Z, Is.EqualTo(8.73f));
            Assert.That(world.SpawnPoint.Pitch, Is.EqualTo(3.14f));
            Assert.That(world.SpawnPoint.Yaw, Is.EqualTo(42f));
            Assert.That(world.Type, Is.EqualTo(WorldType.Nether));
        }

        [Test]
        public void GivenALegacyWorld_WhenGettingAWorld_ThenNewPropertiesUseDefaults()
        {
            repositoryMock
                .Setup(repository => repository.Get("main"))
                .Returns(new WorldDataObject { Id = "main" });

            World world = worldService.GetWorld("main");

            Assert.That(world.HasWebMap, Is.False);
            Assert.That(world.SpawnPoint, Is.Null);
            Assert.That(world.Type, Is.EqualTo(WorldType.Overworld));
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

            World[] worlds = worldService.GetAllWorlds().ToArray();

            Assert.That(worlds, Has.Length.EqualTo(2));
            Assert.That(worlds[0].Type, Is.EqualTo(WorldType.Nether));
            Assert.That(worlds[1].Type, Is.EqualTo(WorldType.Overworld));
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
            Assert.That(capturedEntity.HasWebMap);
            Assert.That(capturedEntity.SpawnPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Type, Is.EqualTo("nether"));
        }

        [Test]
        public void GivenNewPropertyValues_WhenUpdatingAWorld_ThenAllValuesAreApplied()
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
                Identifier = "main",
                HasWebMap = false,
                SpawnPoint = new()
                {
                    World = "world_the_end",
                    X = 42f,
                    Y = 96f,
                    Z = 128f,
                    Pitch = 6.13f,
                    Yaw = 8.73f
                },
                Type = "end"
            });

            Assert.That(capturedEntity.HasWebMap, Is.False);
            Assert.That(capturedEntity.SpawnPoint.World, Is.EqualTo("world_the_end"));
            Assert.That(capturedEntity.SpawnPoint.X, Is.EqualTo(42f));
            Assert.That(capturedEntity.SpawnPoint.Y, Is.EqualTo(96f));
            Assert.That(capturedEntity.SpawnPoint.Z, Is.EqualTo(128f));
            Assert.That(capturedEntity.SpawnPoint.Pitch, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.SpawnPoint.Yaw, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.Type, Is.EqualTo("end"));
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
            HasWebMap = true,
            SpawnPoint = new CoordinatesDataObject
            {
                World = "world",
                X = 6.13f,
                Y = 64f,
                Z = 8.73f,
                Pitch = 3.14f,
                Yaw = 42f
            },
            Type = "nether",
            Name = new LocalisedStringDataObject
            {
                English = "Main World",
                German = "Hauptwelt",
                Romanian = "Lumea Principala"
            }
        };
    }
}
