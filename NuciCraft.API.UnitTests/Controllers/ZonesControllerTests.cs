using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using NuciCraft.API.Controllers;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class ZonesControllerTests
    {
        private Mock<IZoneService> serviceMock;
        private ZonesController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IZoneService>();
            controller = new ZonesController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(ZonesController)
                .GetMethod(nameof(ZonesController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{zoneIdentifier}"));
        }

        [Test]
        public void GivenTheDeleteAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(ZonesController)
                .GetMethod(nameof(ZonesController.Delete));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{zoneIdentifier}"));
            Assert.That(actionMethod.GetCustomAttribute<HttpDeleteAttribute>(), Is.Not.Null);
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingAZone_ThenTheServiceReceivesTheRequest()
        {
            AddZoneRequest request = new()
            {
                Identifier = "solara"
            };

            OkObjectResult result = controller.Add(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Add(request), Times.Once);
        }

        [Test]
        public void GivenAZoneIdentifier_WhenDeletingAZone_ThenTheServiceReceivesTheIdentifier()
        {
            OkObjectResult result = controller.Delete("solara") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Delete("solara"), Times.Once);
        }

        [Test]
        public void GivenAZoneIdentifier_WhenGettingAZone_ThenTheZoneIsReturned()
        {
            Zone zone = new()
            {
                Identifier = "solara"
            };
            serviceMock
                .Setup(service => service.GetZone("solara"))
                .Returns(zone);

            OkObjectResult result = controller.Get("solara") as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(zone));
        }

        [Test]
        public void GivenZones_WhenGettingAllZones_ThenTheZonesAreReturned()
        {
            IEnumerable<Zone> zones = [new() { Identifier = "solara" }];
            serviceMock
                .Setup(service => service.GetAllZones())
                .Returns(zones);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(zones));
        }

        [Test]
        public void GivenCoordinates_WhenGettingContainingZones_ThenTheirIdentifiersAreReturned()
        {
            IEnumerable<string> zoneIdentifiers = ["solara", "nucilandia"];
            GetZonesContainingCoordinatesRequest request = new()
            {
                World = "world",
                X = 64f,
                Y = 72f,
                Z = 128f
            };
            serviceMock
                .Setup(service => service.GetZoneIdentifiersContainingCoordinates(It.Is<CoordinatesDataObject>(
                    coordinates => string.Equals(coordinates.World, "world", StringComparison.Ordinal)
                        && coordinates.X == 64f
                        && coordinates.Y == 72f
                        && coordinates.Z == 128f)))
                .Returns(zoneIdentifiers);

            OkObjectResult result = controller.GetContainingCoordinates(request) as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(zoneIdentifiers));
            serviceMock.Verify(
                service => service.GetZoneIdentifiersContainingCoordinates(It.Is<CoordinatesDataObject>(
                    coordinates => string.Equals(coordinates.World, "world", StringComparison.Ordinal)
                        && coordinates.X == 64f
                        && coordinates.Y == 72f
                        && coordinates.Z == 128f)),
                Times.Once);
        }

        [Test]
        public void GivenTheGetContainingCoordinatesAction_WhenInspectingItsRoute_ThenItUsesTheByCoordinatesSegmentAndGetMethod()
        {
            MethodInfo actionMethod = typeof(ZonesController)
                .GetMethod(nameof(ZonesController.GetContainingCoordinates));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-coordinates"));
            Assert.That(actionMethod.GetCustomAttribute<HttpGetAttribute>(), Is.Not.Null);
        }

        [Test]
        public void GivenAZoneIdentifier_WhenPatchingAZone_ThenTheIdentifierIsAppliedToTheRequest()
        {
            PatchZoneRequest request = new();

            OkObjectResult result = controller.PatchByIdentifier("solara", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("solara"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }
    }
}
