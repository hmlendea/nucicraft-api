using System.Collections.Generic;
using System.Linq;
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
    public sealed class PlayersControllerTests
    {
        private Mock<IPlayerService> serviceMock;
        private PlayersController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IPlayerService>();
            controller = new PlayersController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenTheGetAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.Get), [typeof(string)]);
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{identifier}"));
        }

        [Test]
        public void GivenTheGetByUsernameAction_WhenInspectingItsRoute_ThenItUsesTheByUsernameSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.GetByUsername));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-username/{username}"));
        }

        [Test]
        public void GivenTheGetByOfflineUuidAction_WhenInspectingItsRoute_ThenItUsesTheByOfflineUuidSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.GetByOfflineUuid));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-offline-uuid/{offlineUUID}"));
        }

        [Test]
        public void GivenTheGetByOnlineUuidAction_WhenInspectingItsRoute_ThenItUsesTheByOnlineUuidSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.GetByOnlineUuid));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-online-uuid/{onlineUUID}"));
        }

        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{playerIdentifier}"));
        }

        [Test]
        public void GivenThePatchByUsernameAction_WhenInspectingItsRoute_ThenItUsesTheByUsernameSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.PatchByUsername));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-username/{username}"));
        }

        [Test]
        public void GivenThePatchByOfflineUuidAction_WhenInspectingItsRoute_ThenItUsesTheByOfflineUuidSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.PatchByOfflineUuid));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-offline-uuid/{offlineUUID}"));
        }

        [Test]
        public void GivenThePatchByOnlineUuidAction_WhenInspectingItsRoute_ThenItUsesTheByOnlineUuidSegment()
        {
            MethodInfo actionMethod = typeof(PlayersController)
                .GetMethod(nameof(PlayersController.PatchByOnlineUuid));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("by-online-uuid/{onlineUUID}"));
        }

        [Test]
        public void GivenARegisterRequest_WhenRegisteringAPlayer_ThenTheServiceReceivesTheRequest()
        {
            RegisterPlayerRequest request = new()
            {
                Username = "IlarionPintilie"
            };

            OkObjectResult result = controller.Register(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Register(request), Times.Once);
        }

        [Test]
        public void GivenAnIdentifier_WhenGettingAPlayer_ThenTheIdentifierSelectorIsUsed()
        {
            ArrangePlayerLookup();

            OkObjectResult result = controller.Get("613") as OkObjectResult;

            Assert.That(result.Value, Is.TypeOf<GetPlayerResponse>());
            serviceMock.Verify(
                service => service.Get(It.Is<GetPlayerRequest>(request =>
                    string.Equals(request.Identifier, "613") &&
                    string.Equals(request.Username, null) &&
                    string.Equals(request.OfflineUUID, null) &&
                    string.Equals(request.OnlineUUID, null))),
                Times.Once);
        }

        [Test]
        public void GivenAUsername_WhenGettingAPlayer_ThenTheUsernameSelectorIsUsed()
        {
            ArrangePlayerLookup();

            OkObjectResult result = controller.GetByUsername("IlarionPintilie") as OkObjectResult;

            Assert.That(result.Value, Is.TypeOf<GetPlayerResponse>());
            serviceMock.Verify(
                service => service.Get(It.Is<GetPlayerRequest>(request =>
                    string.Equals(request.Identifier, null) &&
                    string.Equals(request.Username, "IlarionPintilie") &&
                    string.Equals(request.OfflineUUID, null) &&
                    string.Equals(request.OnlineUUID, null))),
                Times.Once);
        }

        [Test]
        public void GivenAnOfflineUUID_WhenGettingAPlayer_ThenTheOfflineUUIDSelectorIsUsed()
        {
            ArrangePlayerLookup();

            OkObjectResult result = controller.GetByOfflineUuid("61300000-0000-3000-8000-000000000000") as OkObjectResult;

            Assert.That(result.Value, Is.TypeOf<GetPlayerResponse>());
            serviceMock.Verify(
                service => service.Get(It.Is<GetPlayerRequest>(request =>
                    string.Equals(request.Identifier, null) &&
                    string.Equals(request.Username, null) &&
                    string.Equals(
                        request.OfflineUUID,
                        "61300000-0000-3000-8000-000000000000") &&
                    string.Equals(request.OnlineUUID, null))),
                Times.Once);
        }

        [Test]
        public void GivenAnOnlineUUID_WhenGettingAPlayer_ThenTheOnlineUUIDSelectorIsUsed()
        {
            ArrangePlayerLookup();

            OkObjectResult result = controller.GetByOnlineUuid("87300000-0000-0000-0000-000000000000") as OkObjectResult;

            Assert.That(result.Value, Is.TypeOf<GetPlayerResponse>());
            serviceMock.Verify(
                service => service.Get(It.Is<GetPlayerRequest>(request =>
                    string.Equals(request.Identifier, null) &&
                    string.Equals(request.Username, null) &&
                    string.Equals(request.OfflineUUID, null) &&
                    string.Equals(
                        request.OnlineUUID,
                        "87300000-0000-0000-0000-000000000000"))),
                Times.Once);
        }

        [Test]
        public void GivenPlayers_WhenGettingAllPlayers_ThenPlayerResponsesAreReturned()
        {
            IEnumerable<Player> players = [BuildPlayer()];
            serviceMock
                .Setup(service => service.GetAll())
                .Returns(players);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;
            IEnumerable<GetPlayerResponse> playerResponses = response.Content as IEnumerable<GetPlayerResponse>;
            GetPlayerResponse playerResponse = playerResponses.Single();

            Assert.That(playerResponse.Username, Is.EqualTo("IlarionPintilie"));
            Assert.That(playerResponse.DisplayName, Is.EqualTo("IlarionPintilie"));
        }

        [Test]
        public void GivenNoPlayers_WhenGettingAllPlayers_ThenAnEmptyResponseCollectionIsReturned()
        {
            serviceMock
                .Setup(service => service.GetAll())
                .Returns([]);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;
            IEnumerable<GetPlayerResponse> playerResponses = response.Content as IEnumerable<GetPlayerResponse>;

            Assert.That(playerResponses, Is.Empty);
            serviceMock.Verify(service => service.GetAll(), Times.Once);
        }

        [Test]
        public void GivenMultiplePlayers_WhenGettingAllPlayers_ThenTheirResponseOrderIsPreserved()
        {
            Player firstPlayer = BuildPlayer();
            Player secondPlayer = new()
            {
                Identifier = "873",
                Username = "solaire_of_astora"
            };
            serviceMock
                .Setup(service => service.GetAll())
                .Returns([firstPlayer, secondPlayer]);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            GetResponse response = result.Value as GetResponse;
            IEnumerable<GetPlayerResponse> playerResponses = response.Content as IEnumerable<GetPlayerResponse>;

            Assert.That(
                playerResponses.Select(playerResponse => playerResponse.Username),
                Is.EqualTo([firstPlayer.Username, secondPlayer.Username]));
        }

        [Test]
        public void GivenAnIdentifier_WhenPatchingAPlayer_ThenTheIdentifierSelectorIsApplied()
        {
            PatchPlayerRequest request = new()
            {
                Identifier = "873"
            };

            OkObjectResult result = controller.PatchByIdentifier("613", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("613"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        [Test]
        public void GivenAUsername_WhenPatchingAPlayer_ThenTheUsernameSelectorIsApplied()
        {
            PatchPlayerRequest request = new()
            {
                Username = "DummyUser"
            };

            OkObjectResult result = controller.PatchByUsername("IlarionPintilie", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Username, Is.EqualTo("IlarionPintilie"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        [Test]
        public void GivenAnOfflineUUID_WhenPatchingAPlayer_ThenTheOfflineUUIDSelectorIsApplied()
        {
            PatchPlayerRequest request = new()
            {
                OfflineUUID = "87300000-0000-3000-8000-000000000000"
            };

            OkObjectResult result = controller.PatchByOfflineUuid(
                "61300000-0000-3000-8000-000000000000",
                request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(
                request.OfflineUUID,
                Is.EqualTo("61300000-0000-3000-8000-000000000000"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        [Test]
        public void GivenAnOnlineUUID_WhenPatchingAPlayer_ThenTheOnlineUUIDSelectorIsApplied()
        {
            PatchPlayerRequest request = new()
            {
                OnlineUUID = "61300000-0000-0000-0000-000000000000"
            };

            OkObjectResult result = controller.PatchByOnlineUuid(
                "87300000-0000-0000-0000-000000000000",
                request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(
                request.OnlineUUID,
                Is.EqualTo("87300000-0000-0000-0000-000000000000"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        private void ArrangePlayerLookup()
            => serviceMock
                .Setup(service => service.Get(It.IsAny<GetPlayerRequest>()))
                .Returns(BuildPlayer());

        private static Player BuildPlayer() => new()
        {
            Identifier = "613",
            Username = "IlarionPintilie"
        };
    }
}
