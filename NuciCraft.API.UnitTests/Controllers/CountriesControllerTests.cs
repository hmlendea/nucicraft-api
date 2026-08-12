using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using NuciCraft.API.Controllers;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class CountriesControllerTests
    {
        [Test]
        public void GivenThePatchByIdentifierAction_WhenInspectingItsRoute_ThenItUsesTheIdentifierSegment()
        {
            MethodInfo actionMethod = typeof(CountriesController)
                .GetMethod(nameof(CountriesController.PatchByIdentifier));
            RouteAttribute routeAttribute = actionMethod
                .GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute, Is.Not.Null);
            Assert.That(routeAttribute.Template, Is.EqualTo("{countryIdentifier}"));
        }
    }
}
