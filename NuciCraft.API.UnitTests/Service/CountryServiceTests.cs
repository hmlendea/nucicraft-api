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
    public sealed class CountryServiceTests
    {
        private Mock<IFileRepository<CountryDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private CountryService countryService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<CountryDataObject>>();
            loggerMock = new Mock<ILogger>();
            countryService = new CountryService(repositoryMock.Object, loggerMock.Object);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingACountry_ThenTheCountryIsPersisted()
        {
            CountryDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<CountryDataObject>()))
                .Callback<CountryDataObject>(entity => capturedEntity = entity);

            AddCountryRequest request = new()
            {
                Identifier = "nucilandia",
                Name = new() { English = "Nucilandia", Romanian = "Nucilandia" },
                LeaderTitle = new() { English = "President", Romanian = "Presedinte" },
                Leader = "IlarionPintilie"
            };

            countryService.Add(request);

            Assert.That(capturedEntity.Id, Is.EqualTo("nucilandia"));
            Assert.That(capturedEntity.Name.English, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("President"));
            Assert.That(capturedEntity.LeaderTitle.Romanian, Is.EqualTo("Presedinte"));
            Assert.That(capturedEntity.Leader, Is.EqualTo("IlarionPintilie"));
            Assert.That(capturedEntity.CreatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingACountry_ThenSaveChangesIsInvoked()
        {
            AddCountryRequest request = new()
            {
                Identifier = "nucilandia"
            };

            countryService.Add(request);

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenAddingACountry_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<CountryDataObject>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => countryService.Add(new AddCountryRequest { Identifier = "nucilandia" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAnExistingCountry_WhenGettingACountry_ThenTheCountryIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Returns(BuildCountryDataObject());

            Country country = countryService.Get("nucilandia");

            Assert.That(country.Identifier, Is.EqualTo("nucilandia"));
            Assert.That(country.Name.English, Is.EqualTo("Nucilandia"));
            Assert.That(country.LeaderTitle.English, Is.EqualTo("President"));
            Assert.That(country.Leader, Is.EqualTo("IlarionPintilie"));
        }

        [Test]
        public void GivenCountriesInTheRepository_WhenGettingAllCountries_ThenAllCountriesAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([
                    BuildCountryDataObject(),
                    new CountryDataObject
                    {
                        Id = "romania",
                        Name = new LocalisedStringDataObject { English = "Romania" },
                        LeaderTitle = new LocalisedStringDataObject { English = "President" },
                        Leader = "DummyUser"
                    }
                ]);

            List<Country> countries = countryService.GetAll().ToList();

            Assert.That(countries, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenARequestWithPartialFields_WhenUpdatingACountry_ThenOnlyProvidedFieldsAreUpdated()
        {
            CountryDataObject original = BuildCountryDataObject();
            CountryDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<CountryDataObject>()))
                .Callback<CountryDataObject>(entity => capturedEntity = entity);

            UpdateCountryRequest request = new()
            {
                CountryIdentifier = "nucilandia",
                Name = new()
                {
                    Romanian = "Nucilandia"
                }
            };

            countryService.Update(request);

            Assert.That(capturedEntity.Name.English, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.Name.Romanian, Is.EqualTo("Nucilandia"));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo(original.LeaderTitle.English));
            Assert.That(capturedEntity.Leader, Is.EqualTo(original.Leader));
        }

        [Test]
        public void GivenARequestWithAllFields_WhenUpdatingACountry_ThenAllFieldsAreUpdated()
        {
            CountryDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Returns(BuildCountryDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<CountryDataObject>()))
                .Callback<CountryDataObject>(entity => capturedEntity = entity);

            UpdateCountryRequest request = new()
            {
                CountryIdentifier = "nucilandia",
                Name = new() { English = "New California Republic" },
                LeaderTitle = new() { English = "Chancellor" },
                Leader = "DummyUser"
            };

            countryService.Update(request);

            Assert.That(capturedEntity.Name.English, Is.EqualTo("New California Republic"));
            Assert.That(capturedEntity.LeaderTitle.English, Is.EqualTo("Chancellor"));
            Assert.That(capturedEntity.Leader, Is.EqualTo("DummyUser"));
        }

        [Test]
        public void GivenNoIdentifier_WhenUpdatingACountry_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => countryService.Update(new UpdateCountryRequest
                {
                    Leader = "DummyUser"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingACountry_ThenUpdatedDTIsStamped()
        {
            CountryDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Returns(BuildCountryDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<CountryDataObject>()))
                .Callback<CountryDataObject>(entity => capturedEntity = entity);

            countryService.Update(new UpdateCountryRequest
            {
                CountryIdentifier = "nucilandia"
            });

            Assert.That(capturedEntity.UpdatedDT, Is.Not.Null);
        }

        [Test]
        public void GivenAValidRequest_WhenUpdatingACountry_ThenSaveChangesIsInvoked()
        {
            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Returns(BuildCountryDataObject());

            countryService.Update(new UpdateCountryRequest
            {
                CountryIdentifier = "nucilandia"
            });

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenUpdatingACountry_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("nucilandia"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => countryService.Update(new UpdateCountryRequest { CountryIdentifier = "nucilandia" }),
                Throws.TypeOf<InvalidOperationException>());
        }

        private static CountryDataObject BuildCountryDataObject() => new()
        {
            Id = "nucilandia",
            Name = new LocalisedStringDataObject
            {
                English = "Nucilandia",
                German = "Nucilandien",
                Romanian = "Nucilandia"
            },
            LeaderTitle = new LocalisedStringDataObject
            {
                English = "President",
                German = "Prasident"
            },
            Leader = "IlarionPintilie"
        };
    }
}
