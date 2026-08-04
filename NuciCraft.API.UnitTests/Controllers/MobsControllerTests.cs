using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using NuciCraft.API.Controllers;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class MobsControllerTests
    {
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
    }
}