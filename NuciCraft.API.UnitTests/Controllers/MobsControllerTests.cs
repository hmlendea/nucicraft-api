using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using NuciCraft.API.Controllers;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class MobsControllerTests
    {
        private Mock<IMobService> serviceMock;
        private MobsController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IMobService>();
            controller = new MobsController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenThePrimaryRandomNameAction_WhenInspectingItsRoute_ThenTheMobTypeSegmentPrecedesTheActionName()
        {
            MethodInfo actionMethod = typeof(MobsController)
                .GetMethod(nameof(MobsController.GetRandomMobName));
            HttpGetAttribute httpGetAttribute = actionMethod
                .GetCustomAttribute<HttpGetAttribute>();

            Assert.That(httpGetAttribute, Is.Not.Null);
            Assert.That(httpGetAttribute.Template, Is.EqualTo("{mobType}/random-name"));
        }

        [Test]
        public void GivenAMobType_WhenGettingARandomMobName_ThenTheGeneratedNameIsReturned()
        {
            GetMobNameRequest capturedRequest = null;
            serviceMock
                .Setup(service => service.GetRandomMobName(It.IsAny<GetMobNameRequest>()))
                .Callback<GetMobNameRequest>(request => capturedRequest = request)
                .Returns("Ilarion");

            OkObjectResult result = controller.GetRandomMobName("villager") as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(capturedRequest.MobType, Is.EqualTo("villager"));
            Assert.That(response.Content, Is.EqualTo("Ilarion"));
        }
    }
}