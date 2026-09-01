using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

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
    public sealed class ZoneServiceTests
    {
        private static string RomaniaTimeZoneId => "Europe/Bucharest";

        private static string ValidateBoundsMethodName => "ValidateBounds";

        private Mock<IFileRepository<ZoneDataObject>> repositoryMock;
        private Mock<IFileRepository<WorldDataObject>> worldRepositoryMock;
        private Mock<IFileRepository<ZoneTypeDataObject>> zoneTypeRepositoryMock;
        private Mock<ILogger> loggerMock;
        private ZoneService zoneService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<ZoneDataObject>>();
            worldRepositoryMock = new Mock<IFileRepository<WorldDataObject>>();
            zoneTypeRepositoryMock = new Mock<IFileRepository<ZoneTypeDataObject>>();
            loggerMock = new Mock<ILogger>();
            zoneService = new ZoneService(
                repositoryMock.Object,
                worldRepositoryMock.Object,
                zoneTypeRepositoryMock.Object,
                loggerMock.Object);

            worldRepositoryMock
                .Setup(repository => repository.Get(It.IsAny<string>()))
                .Returns<string>(worldIdentifier => new WorldDataObject { Id = worldIdentifier });

            zoneTypeRepositoryMock
                .Setup(repository => repository.Get(It.IsAny<string>()))
                .Returns<string>(zoneTypeIdentifier => new ZoneTypeDataObject { Id = zoneTypeIdentifier });
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
                World = "world",
                Name = new() { English = "Solara Portal Hub" },
                Nickname = new() { English = "Portal Hub" },
                Type = "district",
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
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world",
                        X = 96f,
                        Y = 96f,
                        Z = -96f,
                        Pitch = 0f,
                        Yaw = 179.9f,
                    },
                    SecondCorner = new()
                    {
                        World = "world",
                        X = 160f,
                        Y = 48f,
                        Z = 0f,
                        Pitch = 0f,
                        Yaw = 179.9f,
                    }
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
            Assert.That(capturedEntity.Type, Is.EqualTo("district"));
            Assert.That(capturedEntity.County, Is.EqualTo("Solara"));
            Assert.That(capturedEntity.Region, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.Country, Is.EqualTo("Roman Republic"));
            Assert.That(capturedEntity.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo("2026-08-10"));
            Assert.That(capturedEntity.Owners, Is.EqualTo(["Hori873"]));
            Assert.That(capturedEntity.Creators, Is.EqualTo(["Hori873"]));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(["DummyUser"]));
            Assert.That(capturedEntity.TeleportationPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.TeleportationPoint.X, Is.EqualTo(120f));
            Assert.That(capturedEntity.TeleportationPoint.Y, Is.EqualTo(64f));
            Assert.That(capturedEntity.TeleportationPoint.Z, Is.EqualTo(-48f));
            Assert.That(capturedEntity.TeleportationPoint.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.TeleportationPoint.Yaw, Is.EqualTo(180f));
            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(96f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(96f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(-96f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Yaw, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(160f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(48f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Yaw, Is.EqualTo(0f));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("Mayor"));
            Assert.That(capturedEntity.Population, Is.EqualTo(128));
            Assert.That(capturedEntity.MapLink, Is.EqualTo("https://nucilandia.ro/map/solara_portal_hub"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://nucilandia.ro/wiki/solara_portal_hub"));
            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenASingleOwnerAndNoCreators_WhenAddingAZone_ThenTheOwnerIsSetAsTheSoleCreator()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Owners = ["Hori873"],
                Creators = null,
                Bounds = BuildZoneBoundsDataObject()
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Creators, Is.EqualTo(["Hori873"]));
        }

        [Test]
        public void GivenMultipleOwnersAndNoCreators_WhenAddingAZone_ThenCreatorsRemainUnset()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Owners = ["Hori873", "DummyUser"],
                Creators = null,
                Bounds = BuildZoneBoundsDataObject()
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Creators, Is.Null);
        }

        [Test]
        public void GivenASingleOwnerAndCreators_WhenAddingAZone_ThenProvidedCreatorsArePreserved()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Owners = ["Hori873"],
                Creators = ["DummyUser"],
                Bounds = BuildZoneBoundsDataObject()
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Creators, Is.EqualTo(["DummyUser"]));
        }

        [Test]
        public void GivenANullCreationDate_WhenAddingAZone_ThenCurrentDateWithUncertaintySuffixIsSet()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            DateTimeOffset callStartTime = GetRomaniaNow();

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                CreationDate = null,
                Bounds = BuildZoneBoundsDataObject()
            };

            zoneService.Add(request);

            DateTimeOffset callEndTime = GetRomaniaNow();

            string expectedCreationDateAtStart = string.Concat(
                callStartTime.ToString(
                    "yyyy'-'MM'-'dd",
                    CultureInfo.InvariantCulture),
                " (?)");
            string expectedCreationDateAtEnd = string.Concat(
                callEndTime.ToString(
                    "yyyy'-'MM'-'dd",
                    CultureInfo.InvariantCulture),
                " (?)");

            Assert.That(
                capturedEntity.CreationDate,
                Is.EqualTo(expectedCreationDateAtStart)
                    .Or.EqualTo(expectedCreationDateAtEnd));
        }

        [Test]
        public void GivenAWhitespaceCreationDate_WhenAddingAZone_ThenCurrentDateWithUncertaintySuffixIsSet()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            DateTimeOffset callStartTime = GetRomaniaNow();

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                CreationDate = "  ",
                Bounds = BuildZoneBoundsDataObject()
            };

            zoneService.Add(request);

            DateTimeOffset callEndTime = GetRomaniaNow();

            string expectedCreationDateAtStart = string.Concat(
                callStartTime.ToString(
                    "yyyy'-'MM'-'dd",
                    CultureInfo.InvariantCulture),
                " (?)");
            string expectedCreationDateAtEnd = string.Concat(
                callEndTime.ToString(
                    "yyyy'-'MM'-'dd",
                    CultureInfo.InvariantCulture),
                " (?)");

            Assert.That(
                capturedEntity.CreationDate,
                Is.EqualTo(expectedCreationDateAtStart)
                    .Or.EqualTo(expectedCreationDateAtEnd));
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAZone_ThenSaveChangesIsInvoked()
        {
            zoneService.Add(new AddZoneRequest
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Bounds = BuildZoneBoundsDataObject()
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
                () => zoneService.Add(new AddZoneRequest
                {
                    Identifier = "solara_portal_hub",
                    World = "world",
                    Type = "unknown",
                    Bounds = BuildZoneBoundsDataObject()
                }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenANullRequest_WhenAddingAZone_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => zoneService.Add(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenRequestWithoutBounds_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world"
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithoutWorld_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = null,
                Bounds = BuildZoneBoundsDataObject()
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenARequestWithoutAZoneType_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = BuildZoneBoundsDataObject()
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenARequestWithAnUnknownZoneType_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            zoneTypeRepositoryMock
                .Setup(repository => repository.Get("unknown-zone-type"))
                .Throws<KeyNotFoundException>();

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown-zone-type",
                Bounds = BuildZoneBoundsDataObject()
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithUnknownWorld_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            worldRepositoryMock
                .Setup(repository => repository.Get("unknown-world"))
                .Throws<KeyNotFoundException>();

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "unknown-world",
                Bounds = BuildZoneBoundsDataObject()
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithBoundsInDifferentWorlds_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world",
                        X = 4,
                        Y = 8,
                        Z = 16
                    },
                    SecondCorner = new()
                    {
                        World = "world_nether",
                        X = 32,
                        Y = 42,
                        Z = 48
                    }
                }
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithBoundsWithoutAFirstCorner_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = new()
                {
                    SecondCorner = new()
                    {
                        World = "world"
                    }
                }
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithBoundsWithoutASecondCorner_WhenAddingAZone_ThenAnArgumentExceptionIsThrown()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world"
                    }
                }
            };

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public void GivenARequestWithBoundsWithoutAFirstCornerWorld_WhenAddingAZone_ThenAnArgumentExceptionIsThrown(
            string world)
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = BuildZoneBoundsDataObject()
            };
            request.Bounds.FirstCorner.World = world;

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public void GivenARequestWithBoundsWithoutASecondCornerWorld_WhenAddingAZone_ThenAnArgumentExceptionIsThrown(
            string world)
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Bounds = BuildZoneBoundsDataObject()
            };
            request.Bounds.SecondCorner.World = world;

            Assert.That(
                () => zoneService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenRequestWithReversedBoundsCorners_WhenAddingAZone_ThenBoundsAreNormalised()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Add(new AddZoneRequest
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Bounds = new ZoneBoundsDataObject
                {
                    FirstCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 96,
                        Y = 32,
                        Z = 192,
                    },
                    SecondCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 32,
                        Y = 96,
                        Z = 96,
                    }
                }
            });

            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(32));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(96));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(96));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(96));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(32));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(192));
        }

        [Test]
        public void GivenBoundsWithIdenticalCorners_WhenAddingAZone_ThenTheDegenerateBoundsArePreserved()
        {
            ZoneDataObject capturedEntity = null;
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            CoordinatesDataObject corner = new()
            {
                World = "world",
                X = 42,
                Y = 64,
                Z = 128
            };
            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Bounds = new()
                {
                    FirstCorner = corner,
                    SecondCorner = corner
                }
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(corner.X));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(corner.Y));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(corner.Z));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(corner.X));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(corner.Y));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(corner.Z));
        }

        [Test]
        public void GivenBoundsSpanningNegativeAndPositiveCoordinates_WhenAddingAZone_ThenEveryAxisIsNormalised()
        {
            ZoneDataObject capturedEntity = null;
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            AddZoneRequest request = new()
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world",
                        X = 613,
                        Y = -873,
                        Z = 42
                    },
                    SecondCorner = new()
                    {
                        World = "world",
                        X = -873,
                        Y = 613,
                        Z = -613
                    }
                }
            };

            zoneService.Add(request);

            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(-873));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(613));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(-613));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(613));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(-873));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(42));
        }

        [Test]
        public void GivenRequestWithNonZeroBoundsPitchAndYaw_WhenAddingAZone_ThenBoundsPitchAndYawAreResetToZero()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Add(new AddZoneRequest
            {
                Identifier = "solara_portal_hub",
                World = "world",
                Type = "unknown",
                Bounds = new ZoneBoundsDataObject
                {
                    FirstCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 32,
                        Y = 96,
                        Z = 96,
                        Pitch = 6.13f,
                        Yaw = 8.73f,
                    },
                    SecondCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 96,
                        Y = 48,
                        Z = 192,
                        Pitch = 3.14f,
                        Yaw = 42f,
                    }
                }
            });

            Assert.That(capturedEntity.Bounds.FirstCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Yaw, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Yaw, Is.EqualTo(0f));
        }

        [Test]
        public void GivenAZoneIdentifier_WhenDeletingAZone_ThenTheZoneIsRemovedAndChangesAreSaved()
        {
            zoneService.Delete("flusseland_mall_shop_9");

            repositoryMock.Verify(
                repository => repository.Remove("flusseland_mall_shop_9"),
                Times.Once);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenDeletingAZone_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Remove("flusseland_mall_shop_9"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.Delete("flusseland_mall_shop_9"),
                Throws.TypeOf<InvalidOperationException>());
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Never);
        }

        [Test]
        public void GivenAZoneWithBounds_WhenGettingAZone_ThenBoundsAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            Zone zone = zoneService.GetZone("flusseland_mall_shop_9");

            Assert.That(zone.Bounds, Is.Not.Null);
            Assert.That(zone.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(zone.Bounds.FirstCorner.X, Is.EqualTo(32));
            Assert.That(zone.Bounds.FirstCorner.Y, Is.EqualTo(96));
            Assert.That(zone.Bounds.FirstCorner.Z, Is.EqualTo(96));
            Assert.That(zone.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(zone.Bounds.SecondCorner.X, Is.EqualTo(96));
            Assert.That(zone.Bounds.SecondCorner.Y, Is.EqualTo(48));
            Assert.That(zone.Bounds.SecondCorner.Z, Is.EqualTo(192));
        }

        [Test]
        public void GivenAZoneWithoutBounds_WhenGettingAZone_ThenBoundsRemainNull()
        {
            ZoneDataObject zoneDataObject = BuildZoneDataObject();
            zoneDataObject.Bounds = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(zoneDataObject);

            Zone zone = zoneService.GetZone("flusseland_mall_shop_9");

            Assert.That(zone.Bounds, Is.Null);
        }

        [Test]
        public void GivenAZoneWithoutAFirstBoundsCorner_WhenGettingAZone_ThenTheSecondCornerIsReturned()
        {
            ZoneDataObject zoneDataObject = BuildZoneDataObject();
            zoneDataObject.Bounds.FirstCorner = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(zoneDataObject);

            Zone zone = zoneService.GetZone("flusseland_mall_shop_9");

            Assert.That(zone.Bounds.FirstCorner, Is.Null);
            Assert.That(zone.Bounds.SecondCorner, Is.Not.Null);
        }

        [Test]
        public void GivenAZoneWithoutASecondBoundsCorner_WhenGettingAZone_ThenTheFirstCornerIsReturned()
        {
            ZoneDataObject zoneDataObject = BuildZoneDataObject();
            zoneDataObject.Bounds.SecondCorner = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(zoneDataObject);

            Zone zone = zoneService.GetZone("flusseland_mall_shop_9");

            Assert.That(zone.Bounds.FirstCorner, Is.Not.Null);
            Assert.That(zone.Bounds.SecondCorner, Is.Null);
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAZone_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.GetZone("flusseland_mall_shop_9"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenZonesWithBounds_WhenGettingAllZones_ThenBoundsAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildZoneDataObject()]);

            Zone[] zones = zoneService.GetAllZones().ToArray();

            Assert.That(zones, Has.Length.EqualTo(1));
            Assert.That(zones[0].Bounds, Is.Not.Null);
            Assert.That(zones[0].Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(zones[0].Bounds.SecondCorner.World, Is.EqualTo("world"));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAllZones_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.GetAllZones(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenOverlappingZonesAndCoordinatesOnTheirBounds_WhenGettingContainingZoneIdentifiers_ThenAllMatchingIdentifiersAreReturned()
        {
            ZoneDataObject matchingZone = BuildZoneDataObject();
            matchingZone.Id = "solara";
            ZoneDataObject overlappingZone = BuildZoneDataObject();
            overlappingZone.Id = "nucilandia";
            overlappingZone.Bounds.FirstCorner.X = 96f;
            ZoneDataObject differentWorldZone = BuildZoneDataObject();
            differentWorldZone.Id = "nether";
            differentWorldZone.Bounds.FirstCorner.World = "world_nether";
            differentWorldZone.Bounds.SecondCorner.World = "world_nether";
            ZoneDataObject incompleteBoundsZone = BuildZoneDataObject();
            incompleteBoundsZone.Id = "incomplete";
            incompleteBoundsZone.Bounds.FirstCorner = null;
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([matchingZone, overlappingZone, differentWorldZone, incompleteBoundsZone]);

            string[] zoneIdentifiers = zoneService
                .GetZoneIdentifiersContainingCoordinates(new CoordinatesDataObject
                {
                    World = "world",
                    X = 96f,
                    Y = 48f,
                    Z = 192f
                })
                .ToArray();

            Assert.That(zoneIdentifiers, Is.EqualTo(["solara", "nucilandia"]));
        }

        [Test]
        public void GivenCoordinatesOutsideAllZones_WhenGettingContainingZoneIdentifiers_ThenAnEmptyCollectionIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildZoneDataObject()]);

            string[] zoneIdentifiers = zoneService
                .GetZoneIdentifiersContainingCoordinates(new CoordinatesDataObject
                {
                    World = "world",
                    X = 31f,
                    Y = 48f,
                    Z = 192f
                })
                .ToArray();

            Assert.That(zoneIdentifiers, Is.Empty);
        }

        [Test]
        public void GivenCoordinatesWithoutAWorld_WhenGettingContainingZoneIdentifiers_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => zoneService.GetZoneIdentifiersContainingCoordinates(new CoordinatesDataObject()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenNullCoordinates_WhenGettingContainingZoneIdentifiers_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => zoneService.GetZoneIdentifiersContainingCoordinates(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenARepositoryException_WhenGettingContainingZoneIdentifiers_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.GetZoneIdentifiersContainingCoordinates(new CoordinatesDataObject
                {
                    World = "world"
                }),
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

            PatchZoneRequest request = new()
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
                },
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world_nether",
                        X = 512,
                        Y = 32,
                        Z = 768,
                    },
                    SecondCorner = new()
                    {
                        World = "world_nether",
                        X = 640,
                        Y = 96,
                        Z = 896,
                    }
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
            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(512));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(896));
            Assert.That(capturedEntity.Type, Is.EqualTo(original.Type));
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

            PatchZoneRequest request = new()
            {
                Identifier = "flusseland_mall_shop_9",
                Name = new() { English = "Flusseland Shop 9" },
                Nickname = new() { English = "Shop Nine" },
                Type = "district",
                County = "Solara",
                Region = "Murasaki",
                Country = "Nucilandia",
                World = "world_nether",
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
                Bounds = new()
                {
                    FirstCorner = new()
                    {
                        World = "world",
                        X = 4,
                        Y = 8,
                        Z = 16,
                    },
                    SecondCorner = new()
                    {
                        World = "world",
                        X = 64,
                        Y = 96,
                        Z = 128,
                    }
                },
                LeaderTitle = new() { English = "Mayor" },
                Population = 2048,
                MapLink = "https://dummy-url.com/map",
                WikiUrl = "https://dummy-url.com/wiki"
            };

            zoneService.Update(request);

            Assert.That(capturedEntity.Name.English, Is.EqualTo("Flusseland Shop 9"));
            Assert.That(capturedEntity.Nickname.English, Is.EqualTo("Shop Nine"));
            Assert.That(capturedEntity.Type, Is.EqualTo("district"));
            Assert.That(capturedEntity.County, Is.EqualTo("Solara"));
            Assert.That(capturedEntity.Region, Is.EqualTo("Murasaki"));
            Assert.That(capturedEntity.Country, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.World, Is.EqualTo("world_nether"));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo("2026-08-09"));
            Assert.That(capturedEntity.Owners, Is.EqualTo(["IlarionPintilie"]));
            Assert.That(capturedEntity.Creators, Is.EqualTo(["IlarionPintilie"]));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(["DummyUser"]));
            Assert.That(capturedEntity.TeleportationPoint.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.TeleportationPoint.X, Is.EqualTo(8));
            Assert.That(capturedEntity.TeleportationPoint.Y, Is.EqualTo(16));
            Assert.That(capturedEntity.TeleportationPoint.Z, Is.EqualTo(32));
            Assert.That(capturedEntity.TeleportationPoint.Pitch, Is.EqualTo(3.14f));
            Assert.That(capturedEntity.TeleportationPoint.Yaw, Is.EqualTo(6.13f));
            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(4));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(128));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("Mayor"));
            Assert.That(capturedEntity.Population, Is.EqualTo(2048));
            Assert.That(capturedEntity.MapLink, Is.EqualTo("https://dummy-url.com/map"));
            Assert.That(capturedEntity.WikiUrl, Is.EqualTo("https://dummy-url.com/wiki"));
        }

        [Test]
        public void GivenARequestWithOnlyOneBoundsCorner_WhenUpdatingAZone_ThenExistingOtherCornerIsPreservedAfterNormalisation()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Bounds = new ZoneBoundsDataObject
                {
                    FirstCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 256,
                        Y = 64,
                        Z = 512,
                    }
                }
            });

            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(96));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(64));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(192));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(256));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(48));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(512));
        }

        [Test]
        public void GivenAZoneWithoutExistingBounds_WhenUpdatingBounds_ThenRequestedBoundsAreApplied()
        {
            ZoneDataObject zoneDataObject = BuildZoneDataObject();
            zoneDataObject.Bounds = null;
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(zoneDataObject);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Bounds = BuildZoneBoundsDataObject()
            });

            Assert.That(capturedEntity.Bounds.FirstCorner, Is.Not.Null);
            Assert.That(capturedEntity.Bounds.SecondCorner, Is.Not.Null);
        }

        [Test]
        public void GivenARequestWithBoundsInDifferentWorlds_WhenUpdatingAZone_ThenAnArgumentExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            Assert.That(
                () => zoneService.Update(new PatchZoneRequest
                {
                    Identifier = "flusseland_mall_shop_9",
                    Bounds = new ZoneBoundsDataObject
                    {
                        FirstCorner = new CoordinatesDataObject
                        {
                            World = "world_nether",
                            X = 4,
                            Y = 8,
                            Z = 16,
                        },
                        SecondCorner = new CoordinatesDataObject
                        {
                            World = "world",
                            X = 32,
                            Y = 42,
                            Z = 48,
                        }
                    }
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenARequestWithReversedBoundsCorners_WhenUpdatingAZone_ThenBoundsAreNormalised()
        {
            ZoneDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Bounds = new ZoneBoundsDataObject
                {
                    FirstCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 128,
                        Y = 32,
                        Z = 256,
                    },
                    SecondCorner = new CoordinatesDataObject
                    {
                        World = "world",
                        X = 64,
                        Y = 112,
                        Z = 128,
                    }
                }
            });

            Assert.That(capturedEntity.Bounds.FirstCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.FirstCorner.X, Is.EqualTo(64));
            Assert.That(capturedEntity.Bounds.FirstCorner.Y, Is.EqualTo(112));
            Assert.That(capturedEntity.Bounds.FirstCorner.Z, Is.EqualTo(128));
            Assert.That(capturedEntity.Bounds.FirstCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.FirstCorner.Yaw, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.World, Is.EqualTo("world"));
            Assert.That(capturedEntity.Bounds.SecondCorner.X, Is.EqualTo(128));
            Assert.That(capturedEntity.Bounds.SecondCorner.Y, Is.EqualTo(32));
            Assert.That(capturedEntity.Bounds.SecondCorner.Z, Is.EqualTo(256));
            Assert.That(capturedEntity.Bounds.SecondCorner.Pitch, Is.EqualTo(0f));
            Assert.That(capturedEntity.Bounds.SecondCorner.Yaw, Is.EqualTo(0f));
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

            zoneService.Update(new PatchZoneRequest
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

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9"
            });

            Assert.That(capturedEntity.Name, Is.EqualTo(original.Name));
            Assert.That(capturedEntity.Nickname, Is.EqualTo(original.Nickname));
            Assert.That(capturedEntity.Type, Is.EqualTo(original.Type));
            Assert.That(capturedEntity.County, Is.EqualTo(original.County));
            Assert.That(capturedEntity.Region, Is.EqualTo(original.Region));
            Assert.That(capturedEntity.Country, Is.EqualTo(original.Country));
            Assert.That(capturedEntity.World, Is.EqualTo(original.World));
            Assert.That(capturedEntity.CreationDate, Is.EqualTo(original.CreationDate));
            Assert.That(capturedEntity.Owners, Is.EqualTo(original.Owners));
            Assert.That(capturedEntity.Creators, Is.EqualTo(original.Creators));
            Assert.That(capturedEntity.Leaders, Is.EqualTo(original.Leaders));
            Assert.That(capturedEntity.TeleportationPoint, Is.EqualTo(original.TeleportationPoint));
            Assert.That(capturedEntity.Bounds, Is.EqualTo(original.Bounds));
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

            zoneService.Update(new PatchZoneRequest
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
            zoneService.Update(new PatchZoneRequest { Identifier = "flusseland_mall_shop_9" });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
            Assert.That(DateTimeOffset.Parse(capturedEntity.UpdatedDT), Is.GreaterThanOrEqualTo(callTime));
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingAZone_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Returns(BuildZoneDataObject());

            zoneService.Update(new PatchZoneRequest { Identifier = "flusseland_mall_shop_9" });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingAZone_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("flusseland_mall_shop_9"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => zoneService.Update(new PatchZoneRequest { Identifier = "flusseland_mall_shop_9" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenARequestWithUnknownWorld_WhenUpdatingAZone_ThenAnArgumentExceptionIsThrown()
        {
            worldRepositoryMock
                .Setup(repository => repository.Get("unknown-world"))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => zoneService.Update(new PatchZoneRequest
                {
                    Identifier = "flusseland_mall_shop_9",
                    World = "unknown-world"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenARequestWithAnUnknownZoneType_WhenUpdatingAZone_ThenAnArgumentExceptionIsThrown()
        {
            zoneTypeRepositoryMock
                .Setup(repository => repository.Get("unknown-zone-type"))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => zoneService.Update(new PatchZoneRequest
                {
                    Identifier = "flusseland_mall_shop_9",
                    Type = "unknown-zone-type"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenAnIdentifierSelector_WhenPatchingAZone_ThenTheZoneIsUpdated()
        {
            ZoneDataObject capturedEntity = null;
            ZoneDataObject original = BuildZoneDataObject();

            repositoryMock
            .Setup(repository => repository.Get("flusseland_mall_shop_9"))
            .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Population = 4096
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Population, Is.EqualTo(4096));
        }

        [Test]
        public void GivenAnIdentifierSelector_WhenPatchingAZone_ThenAnotherFieldCanBeUpdated()
        {
            ZoneDataObject capturedEntity = null;
            ZoneDataObject original = BuildZoneDataObject();

            repositoryMock
            .Setup(repository => repository.Get("flusseland_mall_shop_9"))
            .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneDataObject>()))
                .Callback<ZoneDataObject>(entity => capturedEntity = entity);

            zoneService.Update(new PatchZoneRequest
            {
                Identifier = "flusseland_mall_shop_9",
                Country = "Nucilandia"
            });

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Country, Is.EqualTo("Nucilandia"));
        }

        [Test]
        public void GivenNoIdentifier_WhenPatchingAZone_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => zoneService.Update(new PatchZoneRequest
                {
                    Country = "Nucilandia"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenANonExistentIdentifier_WhenPatchingAZone_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("non-existent-zone"))
                .Throws<KeyNotFoundException>();

            Assert.That(
                () => zoneService.Update(new PatchZoneRequest { Identifier = "non-existent-zone" }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenANullRequest_WhenPatchingAZone_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => zoneService.Update(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenNullBounds_WhenValidatingBounds_ThenNoExceptionIsThrown()
            => Assert.That(
                () => InvokeValidateBounds(null),
                Throws.Nothing);

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
            Type = "building",
            County = "Flusseland",
            Region = "Solara",
            Country = "Roman Republic",
            World = "world",
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
            Bounds = new ZoneBoundsDataObject
            {
                FirstCorner = new CoordinatesDataObject
                {
                    World = "world",
                    X = 32,
                    Y = 96,
                    Z = 96,
                },
                SecondCorner = new CoordinatesDataObject
                {
                    World = "world",
                    X = 96,
                    Y = 48,
                    Z = 192,
                }
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

        private static ZoneBoundsDataObject BuildZoneBoundsDataObject() => new()
        {
            FirstCorner = new CoordinatesDataObject
            {
                World = "world",
                X = 32,
                Y = 96,
                Z = 96,
            },
            SecondCorner = new CoordinatesDataObject
            {
                World = "world",
                X = 96,
                Y = 48,
                Z = 192,
            }
        };

        private static DateTimeOffset GetRomaniaNow()
        {
            TimeZoneInfo romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RomaniaTimeZoneId);

            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, romaniaTimeZone);
        }

        private static void InvokeValidateBounds(ZoneBoundsDataObject bounds)
            => typeof(ZoneService)
                .GetMethod(ValidateBoundsMethodName, BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, [bounds]);
    }
}