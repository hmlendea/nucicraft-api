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
    public sealed class ZoneTypeServiceTests
    {
        private Mock<IFileRepository<ZoneTypeDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private ZoneTypeService zoneTypeService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<ZoneTypeDataObject>>();
            loggerMock = new Mock<ILogger>();
            zoneTypeService = new ZoneTypeService(repositoryMock.Object, loggerMock.Object);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAZoneType_ThenTheZoneTypeIsPersisted()
        {
            ZoneTypeDataObject capturedDataObject = null;
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ZoneTypeDataObject>()))
                .Callback<ZoneTypeDataObject>(dataObject => capturedDataObject = dataObject);

            zoneTypeService.Add(new AddZoneTypeRequest
            {
                Identifier = "city",
                Name = new() { English = "City", Romanian = "Oras" }
            });

            Assert.That(capturedDataObject.Id, Is.EqualTo("city"));
            Assert.That(capturedDataObject.Name.English, Is.EqualTo("City"));
            Assert.That(capturedDataObject.Name.Romanian, Is.EqualTo("Oras"));
            Assert.That(capturedDataObject.CreatedDT, Is.Not.Null);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenANullRequest_WhenAddingAZoneType_ThenAnArgumentNullExceptionIsThrown()
            => Assert.That(
                () => zoneTypeService.Add(null),
                Throws.TypeOf<ArgumentNullException>());

        [Test]
        public void GivenAnExistingZoneType_WhenGettingAZoneType_ThenTheZoneTypeIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.Get("city"))
                .Returns(BuildZoneTypeDataObject());

            ZoneType zoneType = zoneTypeService.GetZoneType("city");

            Assert.That(zoneType.Identifier, Is.EqualTo("city"));
            Assert.That(zoneType.Name.English, Is.EqualTo("City"));
            Assert.That(zoneType.Name.Romanian, Is.EqualTo("Oras"));
        }

        [Test]
        public void GivenZoneTypesInTheRepository_WhenGettingAllZoneTypes_ThenAllZoneTypesAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildZoneTypeDataObject(), new() { Id = "castle" }]);

            ZoneType[] zoneTypes = zoneTypeService.GetAllZoneTypes().ToArray();

            Assert.That(zoneTypes, Has.Length.EqualTo(2));
            Assert.That(zoneTypes[1].Identifier, Is.EqualTo("castle"));
        }

        [Test]
        public void GivenAPartialLocalisation_WhenUpdatingAZoneType_ThenOnlyProvidedLocalisationsAreUpdated()
        {
            ZoneTypeDataObject capturedDataObject = null;
            repositoryMock
                .Setup(repository => repository.Get("city"))
                .Returns(BuildZoneTypeDataObject());
            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ZoneTypeDataObject>()))
                .Callback<ZoneTypeDataObject>(dataObject => capturedDataObject = dataObject);

            zoneTypeService.Update(new PatchZoneTypeRequest
            {
                Identifier = "city",
                Name = new() { Romanian = "Oras Mare" }
            });

            Assert.That(capturedDataObject.Name.English, Is.EqualTo("City"));
            Assert.That(capturedDataObject.Name.Romanian, Is.EqualTo("Oras Mare"));
            Assert.That(capturedDataObject.Name.German, Is.EqualTo("Stadt"));
            Assert.That(capturedDataObject.UpdatedDT, Is.Not.Null);
            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenAZoneTypeWithoutAName_WhenUpdatingAZoneType_ThenTheIncomingNameIsApplied()
        {
            repositoryMock
                .Setup(repository => repository.Get("city"))
                .Returns(new ZoneTypeDataObject { Id = "city" });

            zoneTypeService.Update(new PatchZoneTypeRequest
            {
                Identifier = "city",
                Name = new() { English = "City" }
            });

            repositoryMock.Verify(
                repository => repository.Update(It.Is<ZoneTypeDataObject>(
                    dataObject => dataObject.Name.English == "City")),
                Times.Once);
        }

        [Test]
        public void GivenARequestWithoutAnIdentifier_WhenUpdatingAZoneType_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => zoneTypeService.Update(new PatchZoneTypeRequest()),
                Throws.TypeOf<ArgumentException>());

        private static ZoneTypeDataObject BuildZoneTypeDataObject() => new()
        {
            Id = "city",
            Name = new()
            {
                English = "City",
                German = "Stadt",
                Romanian = "Oras"
            }
        };
    }
}