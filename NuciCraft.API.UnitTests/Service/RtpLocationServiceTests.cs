using System;
using System.Collections.Generic;

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
    public class RtpLocationServiceTests
    {
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
            Assert.That(capturedEntity.World, Is.EqualTo(request.World));
            Assert.That(capturedEntity.X, Is.EqualTo(request.X));
            Assert.That(capturedEntity.Y, Is.EqualTo(request.Y));
            Assert.That(capturedEntity.Z, Is.EqualTo(request.Z));
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
            nearbyEntity.X = 100;
            nearbyEntity.Y = 0;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([nearbyEntity]);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            request.X = 0;
            request.Y = 0;

            Assert.That(
                () => rtpLocationService.AddRtpLocation(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenALocationTooCloseToAnotherLocationInTheSameBiome_WhenAddingAnRtpLocation_ThenAnArgumentExceptionIsThrown()
        {
            RtpLocationEntity nearbySameBiomeEntity = BuildRtpLocationEntity();
            nearbySameBiomeEntity.Biome = "Forest";
            nearbySameBiomeEntity.X = 700;
            nearbySameBiomeEntity.Y = 0;

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([nearbySameBiomeEntity]);

            AddRtpLocationRequest request = BuildAddRtpLocationRequest();
            request.Biome = "Forest";
            request.X = 0;
            request.Y = 0;

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
            Assert.That(location.World, Is.EqualTo(entity.World));
            Assert.That(location.X, Is.EqualTo(entity.X));
            Assert.That(location.Y, Is.EqualTo(entity.Y));
            Assert.That(location.Z, Is.EqualTo(entity.Z));
        }

        [Test]
        public void GivenAWorldFilter_WhenGettingAnRtpLocation_ThenALocationFromThatWorldIsReturned()
        {
            RtpLocationEntity solaraEntity = BuildRtpLocationEntity();
            solaraEntity.World = "Solara";

            RtpLocationEntity cratesiaEntity = BuildRtpLocationEntity();
            cratesiaEntity.Id = "other-id";
            cratesiaEntity.World = "Cratesia";

            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([solaraEntity, cratesiaEntity]);

            GetRtpLocationRequest request = new() { World = "Solara" };
            RtpLocation location = rtpLocationService.GetRtpLocation(request);

            Assert.That(location.World, Is.EqualTo("Solara"));
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
            World = "Solara",
            X = 10000,
            Y = 10000,
            Z = 613
        };

        private static RtpLocationEntity BuildRtpLocationEntity() => new()
        {
            Id = "61300000-8730-3000-8000-000000000000",
            Biome = "Forest",
            World = "Solara",
            X = 1000,
            Y = 1000,
            Z = 873
        };
    }
}
