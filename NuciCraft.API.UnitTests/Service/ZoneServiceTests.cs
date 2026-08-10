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
    public class ZoneServiceTests
    {
        private Mock<IFileRepository<ZoneDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private ZoneService zoneService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<ZoneDataObject>>();
            loggerMock = new Mock<ILogger>();
            zoneService = new ZoneService(repositoryMock.Object, loggerMock.Object);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAZone_ThenTheZoneIsPersisted()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                Name = new() { English = "Solara Portal Hub" },
                Nickname = new() { English = "Portal Hub" },
                Level = "district",
                County = "Solara",
                Region = "Nucilandia",
                Country = "Roman Republic",
                CreationDate = "2026-08-10",
                Owners = ["Hori873"],
                Creators = ["Hori873"],
                Leaders = ["DummyUser"],
                TeleportationPoint = new()
                {
                    World = "world",
                    X = 120f,
                    Y = 64f,
                    Z = -48f,
                    Pitch = 0f,
                    Yaw = 180f
                },
                LeaderTitle = new() { English = "Mayor" },
                Population = 128,
                MapLink = "https://nucilandia.ro/map/solara_portal_hub",
                WikiUrl = "https://nucilandia.ro/wiki/solara_portal_hub"
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Id, Is.EqualTo("solara_portal_hub"));
            Assert.That(capturedEntity.Name.English, Is.EqualTo("Solara Portal Hub"));
            Assert.That(capturedEntity.Nickname.English, Is.EqualTo("Portal Hub"));
            Assert.That(capturedEntity.Level, Is.EqualTo("district"));
            Assert.That(capturedEntity.County, Is.EqualTo("Solara"));
            Assert.That(capturedEntity.Region, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.Country, Is.EqualTo("Roman Republic"));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo("2026-08-10"));
            Assert.That(capturedEntity.Owners, Is.EqualTo(new[] { "Hori873" }));
            Assert.That(capturedEntity.Creators, Is.EqualTo(new[] { "Hori873" }));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(new[] { "DummyUser" }));
            Assert.That(capturedEntity.TeleportationPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.TeleportationPoint.X, Is.EqualTo(120f));
            Assert.That(capturedEntity.TeleportationPoint.Y, Is.EqualTo(64f));
            Assert.That(capturedEntity.TeleportationPoint.Z, Is.EqualTo(-48f));
            Assert.That(capturedEntity.TeleportationPoint.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.TeleportationPoint.Yaw, Is.EqualTo(180f));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("Mayor"));
            Assert.That(capturedEntity.Population, Is.EqualTo(128));
            Assert.That(capturedEntity.MapLink, Is.EqualTo("https://nucilandia.ro/map/solara_portal_hub"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://nucilandia.ro/wiki/solara_portal_hub"));
            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAZone_ThenSaveChangesIsInvoked()
        {
            zoneService.Add(new AddZoneRequest
            {
                Identifier = "solara_portal_hub"
            });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenAddingAZone_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.Add(new AddZoneRequest { Identifier = "solara_portal_hub" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenARequestWithPartialFields_WhenUpdatingAZone_ThenOnlyProvidedFieldsAreUpdated()
        {
            ZoneDataObject original = BuildZoneDataObject();
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            UpdateZoneRequest request = new()
            {
                Identifier = "flusseland_mall_shop_9",
                CreationDate = "2026-08-09",
                TeleportationPoint = new()
                {
                    World = "world_nether",
                    X = 613,
                    Y = 64,
                    Z = 873,
                    Pitch = 6.13f,
                    Yaw = 8.73f
                }
            };

            zoneService.Update(request);

            Assert.That(capturedEntity.CreationDate, Is.EqualTo("2026-08-09"));
            Assert.That(capturedEntity.TeleportationPoint.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.TeleportationPoint.X, Is.EqualTo(613));
            Assert.That(capturedEntity.TeleportationPoint.Y, Is.EqualTo(64));
            Assert.That(capturedEntity.TeleportationPoint.Z, Is.EqualTo(873));
            Assert.That(capturedEntity.TeleportationPoint.Pitch, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.TeleportationPoint.Yaw, Is.EqualTo(8.73f));
            Assert.That(capturedEntity.Level, Is.EqualTo(original.Level));
            Assert.That(capturedEntity.Country, Is.EqualTo(original.Country));
            Assert.That(capturedEntity.Population, Is.EqualTo(original.Population));
            Assert.That(capturedEntity.MapLink, Is.EqualTo(original.MapLink));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo(original.WikiUrl));
        }

        [Test]
        public void GivenARequestWithAllFields_WhenUpdatingAZone_ThenAllFieldsAreUpdated()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            UpdateZoneRequest request = new()
            {
                Identifier = "flusseland_mall_shop_9",
                Name = new() { English = "Flusseland Shop 9" },
                Nickname = new() { English = "Shop Nine" },
                Level = "district",
                County = "Solara",
                Region = "Murasaki",
                Country = "Nucilandia",
                CreationDate = "2026-08-09",
                Owners = ["IlarionPintilie"],
                Creators = ["IlarionPintilie"],
                Leaders = ["DummyUser"],
                TeleportationPoint = new()
                {
                    World = "world",
                    X = 8,
                    Y = 16,
                    Z = 32,
                    Pitch = 3.14f,
                    Yaw = 6.13f
                },
                LeaderTitle = new() { English = "Mayor" },
                Population = 2048,
                MapLink = "https://dummy-url.com/map",
                WikiUrl = "https://dummy-url.com/wiki"
            };

            zoneService.Update(request);

            Assert.That(capturedEntity.Name.English, Is.EqualTo("Flusseland Shop 9"));
            Assert.That(capturedEntity.Nickname.English, Is.EqualTo("Shop Nine"));
            Assert.That(capturedEntity.Level, Is.EqualTo("district"));
            Assert.That(capturedEntity.County, Is.EqualTo("Solara"));
            Assert.That(capturedEntity.Region, Is.EqualTo("Murasaki"));
            Assert.That(capturedEntity.Country, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo("2026-08-09"));
            Assert.That(capturedEntity.Owners, Is.EqualTo(new[] { "IlarionPintilie" }));
            Assert.That(capturedEntity.Creators, Is.EqualTo(new[] { "IlarionPintilie" }));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(new[] { "DummyUser" }));
            Assert.That(capturedEntity.TeleportationPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.TeleportationPoint.X, Is.EqualTo(8));
            Assert.That(capturedEntity.TeleportationPoint.Y, Is.EqualTo(16));
            Assert.That(capturedEntity.TeleportationPoint.Z, Is.EqualTo(32));
            Assert.That(capturedEntity.TeleportationPoint.Pitch, Is.EqualTo(3.14f));
            Assert.That(capturedEntity.TeleportationPoint.Yaw, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("Mayor"));
            Assert.That(capturedEntity.Population, Is.EqualTo(2048));
            Assert.That(capturedEntity.MapLink, Is.EqualTo("https://dummy-url.com/map"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://dummy-url.com/wiki"));
        }

        [Test]
        public void GivenARequestWithPopulationSetToZero_WhenUpdatingAZone_ThenPopulationIsUpdatedToZero()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new UpdateZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Population = 0
            });

            Assert.That(capturedEntity.Population, Is.EqualTo(0));
        }

        [Test]
        public void GivenARequestWithNoFields_WhenUpdatingAZone_ThenExistingValuesArePreserved()
        {
            ZoneDataObject original = BuildZoneDataObject();
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new UpdateZoneRequest
            {
                Identifier = "flusseland_mall_shop_9"
            });

            Assert.That(capturedEntity.Name, Is.EqualTo(original.Name));
            Assert.That(capturedEntity.Nickname, Is.EqualTo(original.Nickname));
            Assert.That(capturedEntity.Level, Is.EqualTo(original.Level));
            Assert.That(capturedEntity.County, Is.EqualTo(original.County));
            Assert.That(capturedEntity.Region, Is.EqualTo(original.Region));
            Assert.That(capturedEntity.Country, Is.EqualTo(original.Country));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo(original.CreationDate));
            Assert.That(capturedEntity.Owners, Is.EqualTo(original.Owners));
            Assert.That(capturedEntity.Creators, Is.EqualTo(original.Creators));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(original.Leaders));
            Assert.That(capturedEntity.TeleportationPoint, Is.EqualTo(original.TeleportationPoint));
            Assert.That(capturedEntity.LeaderTitle, Is.EqualTo(original.LeaderTitle));
            Assert.That(capturedEntity.Population, Is.EqualTo(original.Population));
            Assert.That(capturedEntity.MapLink, Is.EqualTo(original.MapLink));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo(original.WikiUrl));
        }

        [Test]
        public void GivenARequestWithRomanianNameOnly_WhenUpdatingAZone_ThenOtherNameLocalisationsArePreserved()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new UpdateZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Name = new LocalisedStringDataObject
                {
                    Romanian = "123"
                }
            });

            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("123"));
            Assert.That(capturedEntity.Name.English, Is.EqualTo("Flusseland Mall Shop 9"));
            Assert.That(capturedEntity.Name.German, Is.EqualTo("Flusseland Einkaufszentrum Laden 9"));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAZone_ThenUpdatedDTIsStamped()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            DateTimeOffset callTime = DateTimeOffset.UtcNow;
            zoneService.Update(new UpdateZoneRequest { Identifier = "flusseland_mall_shop_9" });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAZone_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            zoneService.Update(new UpdateZoneRequest { Identifier = "flusseland_mall_shop_9" });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAZone_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.Update(new UpdateZoneRequest { Identifier = "flusseland_mall_shop_9" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        private static ZoneDataObject BuildZoneDataObject() => new()
        {
            Id = "flusseland_mall_shop_9",
            Name = new LocalisedStringDataObject
            {
                English = "Flusseland Mall Shop 9",
                German = "Flusseland Einkaufszentrum Laden 9",
                Romanian = "Magazinul 9 din Mall-ul Flusseland"
            },
            Nickname = new LocalisedStringDataObject
            {
                English = "Shop 9",
                German = "Laden 9"
            },
            Level = "building",
            County = "Flusseland",
            Region = "Solara",
            Country = "Roman Republic",
            CreationDate = "2024-08-14",
            Owners = ["Blitzkrieg94", "DummyUser"],
            Creators = ["Blitzkrieg94"],
            Leaders = ["IlarionPintilie"],
            TeleportationPoint = new CoordinatesDataObject
            {
                World = "world",
                X = 42,
                Y = 64,
                Z = 128,
                Pitch = 0,
                Yaw = 179.9f
            },
            LeaderTitle = new LocalisedStringDataObject
            {
                English = "Owner",
                German = "Eigentumer"
            },
            Population = 42,
            MapLink = "https://nucilandia.ro/map",
            WikiUrl = "https://nucilandia.ro/wiki"
        };
    }
}