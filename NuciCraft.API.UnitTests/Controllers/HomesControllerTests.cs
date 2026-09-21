using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using NuciCraft.API.Controllers;
using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class HomesControllerTests
    {
        private Mock<IHomeService> serviceMock;
        private HomesController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IHomeService>();
            controller = new HomesController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenTheDeleteAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(HomesController)
                .GetMethod(nameof(HomesController.Delete));
            HttpDeleteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<HttpDeleteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{homeIdentifier}"));
        }

        [Test]
        public void GivenAHomeIdentifier_WhenDeletingAHome_ThenTheServiceReceivesTheIdentifier()
        {
            OkObjectResult result = controller.Delete("home-id") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Delete("home-id"), Times.Once);
        }
    }
}