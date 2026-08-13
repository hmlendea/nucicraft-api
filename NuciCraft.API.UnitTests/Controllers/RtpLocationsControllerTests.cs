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
    public sealed class RtpLocationsControllerTests
    {
        private Mock<IRtpLocationService> serviceMock;
        private RtpLocationsController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IRtpLocationService>();
            controller = new RtpLocationsController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingAnRtpLocation_ThenTheServiceReceivesTheRequest()
        {
            AddRtpLocationRequest request = new()
            {
                Biome = "Forest",
                World = "world",
                X = 613,
                Y = 64,
                Z = 873
            };

            OkObjectResult result = controller.AddRtpLocation(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.AddRtpLocation(request), Times.Once);
        }

        [Test]
        public void GivenAFilterRequest_WhenGettingARandomRtpLocation_ThenTheLocationIsReturned()
        {
            GetRtpLocationRequest request = new()
            {
                Biome = "Forest",
                World = "world"
            };
            RtpLocation location = new()
            {
                Id = "solara"
            };
            serviceMock
                .Setup(service => service.GetRtpLocation(request))
                .Returns(location);

            OkObjectResult result = controller.GetRandomRtpLocation(request) as OkObjectResult;
            GetResponse response = result.Value as GetResponse;

            Assert.That(response.Content, Is.SameAs(location));
        }
    }
}