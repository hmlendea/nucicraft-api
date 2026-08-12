using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using NuciCraft.API.Controllers;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class PlayersControllerTests
    {
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
    }
}
