using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NuciAPI.Responses;

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
        public async Task GivenNoCount_WhenGettingRandomMobNames_ThenOneGeneratedNameIsReturned()
        {
            GetMobNameRequest capturedRequest = null;
            IEnumerable<string> generatedNames = ["Ilarion"];
            serviceMock
                .Setup(service => service.GetRandomMobNameAsync(It.IsAny<GetMobNameRequest>(), It.IsAny<CancellationToken>()))
                .Callback<GetMobNameRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(generatedNames);

            OkObjectResult result = await controller.GetRandomMobName("villager", null, CancellationToken.None) as OkObjectResult;
            NuciApiContentResponse<GetMobNameResponse> response =
                result.Value as NuciApiContentResponse<GetMobNameResponse>;
            GetMobNameResponse content = response.Content;

            Assert.That(capturedRequest.MobType, Is.EqualTo("villager"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
            Assert.That(content.Names, Is.EqualTo(generatedNames));
        }

        [Test]
        public async Task GivenACount_WhenGettingRandomMobNames_ThenThatManyGeneratedNamesAreReturned()
        {
            GetMobNameRequest capturedRequest = null;
            IEnumerable<string> generatedNames =
            [
                "Ilarion",
                "Vasile",
                "Robert",
                "Tibi"
            ];
            serviceMock
                .Setup(service => service.GetRandomMobNameAsync(It.IsAny<GetMobNameRequest>(), It.IsAny<CancellationToken>()))
                .Callback<GetMobNameRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(generatedNames);

            OkObjectResult result = await controller.GetRandomMobName("villager", 4, CancellationToken.None) as OkObjectResult;
            NuciApiContentResponse<GetMobNameResponse> response =
                result.Value as NuciApiContentResponse<GetMobNameResponse>;
            GetMobNameResponse content = response.Content;

            Assert.That(capturedRequest.MobType, Is.EqualTo("villager"));
            Assert.That(capturedRequest.Count, Is.EqualTo(4));
            Assert.That(content.Names, Is.EqualTo(generatedNames));
        }
    }
}