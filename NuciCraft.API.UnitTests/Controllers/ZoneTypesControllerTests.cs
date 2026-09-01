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
    public sealed class ZoneTypesControllerTests
    {
        private Mock<IZoneTypeService> serviceMock;
        private ZoneTypesController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IZoneTypeService>();
            controller = new ZoneTypesController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingAZoneType_ThenTheServiceReceivesTheRequest()
        {
            AddZoneTypeRequest request = new() { Identifier = "city" };

            OkObjectResult result = controller.Add(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Add(request), Times.Once);
        }

        [Test]
        public void GivenAZoneTypeIdentifier_WhenGettingAZoneType_ThenTheZoneTypeIsReturned()
        {
            ZoneType zoneType = new() { Identifier = "city" };
            serviceMock.Setup(service => service.GetZoneType("city")).Returns(zoneType);

            OkObjectResult result = controller.Get("city") as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(zoneType));
        }

        [Test]
        public void GivenZoneTypes_WhenGettingAllZoneTypes_ThenTheZoneTypesAreReturned()
        {
            IEnumerable<ZoneType> zoneTypes = [new() { Identifier = "city" }];
            serviceMock.Setup(service => service.GetAllZoneTypes()).Returns(zoneTypes);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(zoneTypes));
        }

        [Test]
        public void GivenAZoneTypeIdentifier_WhenPatchingAZoneType_ThenTheIdentifierIsAppliedToTheRequest()
        {
            PatchZoneTypeRequest request = new();

            OkObjectResult result = controller.PatchByIdentifier("city", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("city"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(ZoneTypesController)
                .GetMethod(nameof(ZoneTypesController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod.GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute.Template, Is.EqualTo("{zoneTypeIdentifier}"));
        }
    }
}