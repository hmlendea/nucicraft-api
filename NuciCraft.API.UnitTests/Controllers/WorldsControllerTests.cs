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
    public sealed class WorldsControllerTests
    {
        private Mock<IWorldService> serviceMock;
        private WorldsController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IWorldService>();
            controller = new WorldsController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(WorldsController)
                .GetMethod(nameof(WorldsController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{worldIdentifier}"));
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingAWorld_ThenTheServiceReceivesTheRequest()
        {
            AddWorldRequest request = new()
            {
                Identifier = "main"
            };

            OkObjectResult result = controller.Add(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Add(request), Times.Once);
        }

        [Test]
        public void GivenAWorldIdentifier_WhenGettingAWorld_ThenTheWorldIsReturned()
        {
            World world = new()
            {
                Identifier = "main"
            };
            serviceMock
                .Setup(service => service.GetWorld("main"))
                .Returns(world);

            OkObjectResult result = controller.Get("main") as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(world));
        }

        [Test]
        public void GivenWorlds_WhenGettingAllWorlds_ThenTheWorldsAreReturned()
        {
            IEnumerable<World> worlds = [new() { Identifier = "main" }];
            serviceMock
                .Setup(service => service.GetAllWorlds())
                .Returns(worlds);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(worlds));
        }

        [Test]
        public void GivenAWorldIdentifier_WhenPatchingAWorld_ThenTheIdentifierIsAppliedToTheRequest()
        {
            PatchWorldRequest request = new();

            OkObjectResult result = controller.PatchByIdentifier("main", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("main"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }
    }
}
