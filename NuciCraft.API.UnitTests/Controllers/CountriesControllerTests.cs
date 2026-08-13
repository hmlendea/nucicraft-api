using System.Collections.Generic;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using NuciCraft.API.Controllers;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class CountriesControllerTests
    {
        private Mock<ICountryService> serviceMock;
        private CountriesController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<ICountryService>();
            controller = new CountriesController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(CountriesController)
                .GetMethod(nameof(CountriesController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{countryIdentifier}"));
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingACountry_ThenTheServiceReceivesTheRequest()
        {
            AddCountryRequest request = new()
            {
                Identifier = "nucilandia"
            };

            OkObjectResult result = controller.Add(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Add(request), Times.Once);
        }

        [Test]
        public void GivenACountryIdentifier_WhenGettingACountry_ThenTheCountryIsReturned()
        {
            Country country = new()
            {
                Identifier = "nucilandia"
            };
            serviceMock
                .Setup(service => service.Get("nucilandia"))
                .Returns(country);

            OkObjectResult result = controller.Get("nucilandia") as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(country));
        }

        [Test]
        public void GivenCountries_WhenGettingAllCountries_ThenTheCountriesAreReturned()
        {
            IEnumerable<Country> countries = [new() { Identifier = "nucilandia" }];
            serviceMock
                .Setup(service => service.GetAll())
                .Returns(countries);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(countries));
        }

        [Test]
        public void GivenACountryIdentifier_WhenPatchingACountry_ThenTheIdentifierIsAppliedToTheRequest()
        {
            PatchCountryRequest request = new();

            OkObjectResult result = controller.PatchByIdentifier("nucilandia", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("nucilandia"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }
    }
}
