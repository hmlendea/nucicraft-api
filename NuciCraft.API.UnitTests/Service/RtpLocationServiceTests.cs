using System;
using System.Collections.Generic;
using System.Globalization;

using Moq;
using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

using NuciDAL.Repositories;
using NuciLog.Core;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class RtpLocationServiceTests
    {
        private static string TimestampFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

        Mock<IFileRepository<RtpLocationEntity>> repositoryMock;
        Mock<ILogger> loggerMock;
        RtpLocationSettings settings;
        RtpLocationService rtpLocationService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<RtpLocationEntity>>();
            loggerMock = new Mock<ILogger>();

            settings = new()
            {
                MinimumLocationDistance = 613,
                MinimumBiomeLocationDistance = 873
            };

            rtpLocationService = new RtpLocationService(repositoryMock.Object, settings, loggerMock.Object);
        }

        // ── AddRtpLocation ────────────────────────────────────────────────────

        [Test]
        public void GivenAValidRequest_WhenAddingAnRtpLocation_ThenTheEntityIsAddedToTheRepository()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            RtpLocationEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<RtpLocationEntity>()))
                .Callback<RtpLocationEntity>(entity => capturedEntity = entity);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            rtpLocationService.AddRtpLocation(request);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Biome, Is.EqualTo(request.Biome));
            Assert.That(capturedEntity.Coordinates.World, Is.EqualTo(request.World));
            Assert.That(capturedEntity.Coordinates.X, Is.EqualTo(request.X));
            Assert.That(capturedEntity.Coordinates.Y, Is.EqualTo(request.Y));
            Assert.That(capturedEntity.Coordinates.Z, Is.EqualTo(request.Z));
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAnRtpLocation_ThenTheIdIsGenerated()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            RtpLocationEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<RtpLocationEntity>()))
                .Callback<RtpLocationEntity>(entity => capturedEntity = entity);

            rtpLocationService.AddRtpLocation(BuildAddRtpLocationRequest());

            Assert.That(capturedEntity.Id, Is.Not.Null);
            Assert.That(capturedEntity.Id, Is.Not.Empty);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAnRtpLocation_ThenTheCreatedTimestampIsPopulated()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            RtpLocationEntity capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<RtpLocationEntity>()))
                .Callback<RtpLocationEntity>(entity => capturedEntity = entity);

            rtpLocationService.AddRtpLocation(BuildAddRtpLocationRequest());

            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
            Assert.That(capturedEntity.CreatedDT, Is.Not.Empty);
            Assert.That(
                DateTimeOffset.TryParseExact(
                    capturedEntity.CreatedDT,
                    TimestampFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTimeOffset createdTimestamp));
            Assert.That(createdTimestamp, Is.Not.EqualTo(default(DateTimeOffset)));
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAnRtpLocation_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            rtpLocationService.AddRtpLocation(BuildAddRtpLocationRequest());

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenALocationTooCloseToAnotherLocation_WhenAddingAnRtpLocation_ThenAnArgumentExceptionIsThrown()
        {
            RtpLocationEntity nearbyEntity = BuildRtpLocationEntity();
            nearbyEntity.Coordinates.X = 100;
            nearbyEntity.Coordinates.Z = 0;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([nearbyEntity]);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            request.X = 0;
            request.Z = 0;

            Assert.That(
                () => rtpLocationService.AddRtpLocation(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenALocationTooCloseToAnotherLocationInTheSameBiome_WhenAddingAnRtpLocation_ThenAnArgumentExceptionIsThrown()
        {
            RtpLocationEntity nearbySameBiomeEntity = BuildRtpLocationEntity();
            nearbySameBiomeEntity.Biome = "Forest";
            nearbySameBiomeEntity.Coordinates.X = 700;
            nearbySameBiomeEntity.Coordinates.Z = 0;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([nearbySameBiomeEntity]);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            request.Biome = "Forest";
            request.X = 0;
            request.Z = 0;

            Assert.That(
                () => rtpLocationService.AddRtpLocation(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenARepositoryException_WhenAddingAnRtpLocation_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<RtpLocationEntity>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => rtpLocationService.AddRtpLocation(BuildAddRtpLocationRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAnIdenticalXAndZButDifferentWorld_WhenAddingAnRtpLocation_ThenTheLocationIsAccepted()
        {
            RtpLocationEntity existingLocationInOtherWorld = BuildRtpLocationEntity();
            existingLocationInOtherWorld.Coordinates.World = "world_nether";
            existingLocationInOtherWorld.Coordinates.X = 10000;
            existingLocationInOtherWorld.Coordinates.Z = 613;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([existingLocationInOtherWorld]);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            request.World = "world";
            request.X = 10000;
            request.Z = 613;

            Assert.That(
                () => rtpLocationService.AddRtpLocation(request),
                Throws.Nothing);

            repositoryMock.Verify(repository => repository.Add(It.IsAny<RtpLocationEntity>()), Times.Once);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        // ── GetRtpLocation ────────────────────────────────────────────────────

        [Test]
        public void GivenNoFilters_WhenGettingAnRtpLocation_ThenARtpLocationIsReturned()
        {
            RtpLocationEntity entity = BuildRtpLocationEntity();

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([entity]);

            GetRtpLocationRequest request = new() { Biome = null, World = null };
            RtpLocation location = rtpLocationService.GetRtpLocation(request);

            Assert.That(location, Is.Not.Null);
            Assert.That(location.Id, Is.EqualTo(entity.Id));
            Assert.That(location.Biome, Is.EqualTo(entity.Biome));
            Assert.That(location.Coordinates.World, Is.EqualTo(entity.Coordinates.World));
            Assert.That(location.Coordinates.X, Is.EqualTo(entity.Coordinates.X));
            Assert.That(location.Coordinates.Y, Is.EqualTo(entity.Coordinates.Y));
            Assert.That(location.Coordinates.Z, Is.EqualTo(entity.Coordinates.Z));
        }

        [Test]
        public void GivenAWorldFilter_WhenGettingAnRtpLocation_ThenALocationFromThatWorldIsReturned()
        {
            RtpLocationEntity solaraEntity = BuildRtpLocationEntity();
            solaraEntity.Coordinates.World = "world";

            RtpLocationEntity cratesiaEntity = BuildRtpLocationEntity();
            cratesiaEntity.Id = "other-id";
            cratesiaEntity.Coordinates.World = "world_nether";

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([solaraEntity, cratesiaEntity]);

            GetRtpLocationRequest request = new() { World = "world" };
            RtpLocation location = rtpLocationService.GetRtpLocation(request);

            Assert.That(location.Coordinates.World, Is.EqualTo("world"));
        }

        [Test]
        public void GivenABiomeFilter_WhenGettingAnRtpLocation_ThenALocationFromThatBiomeIsReturned()
        {
            RtpLocationEntity forestEntity = BuildRtpLocationEntity();
            forestEntity.Biome = "Forest";

            RtpLocationEntity desertEntity = BuildRtpLocationEntity();
            desertEntity.Id = "other-id";
            desertEntity.Biome = "Desert";

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([forestEntity, desertEntity]);

            GetRtpLocationRequest request = new() { Biome = "Forest" };
            RtpLocation location = rtpLocationService.GetRtpLocation(request);

            Assert.That(location.Biome, Is.EqualTo("Forest"));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAnRtpLocation_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                () => rtpLocationService.GetRtpLocation(new() { Biome = null, World = null }),
                Throws.TypeOf<InvalidOperationException>());
        }

        private static AddRtpLocationRequest BuildAddRtpLocationRequest() => new()
        {
            Biome = "Taiga",
            World = "world",
            X = 10000,
            Y = 10000,
            Z = 613
        };

        private static RtpLocationEntity BuildRtpLocationEntity() => new()
        {
            Id = "61300000-8730-3000-8000-000000000000",
            Biome = "Forest",
            Coordinates = new()
            {
                World = "world",
                X = 1000,
                Y = 1000,
                Z = 873
            }
        };
    }
}
