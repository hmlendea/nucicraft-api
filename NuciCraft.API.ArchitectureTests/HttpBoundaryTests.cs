using System;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

using NUnit.Framework;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class HttpBoundaryTests
    {
        [Test]
        public void GivenControllers_WhenCheckingHttpBoundary_ThenAllUseTheSharedControllerPipeline()
        {
            Type[] controllers = [.. ProductionArchitecture.Assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(ControllerBase).IsAssignableFrom(type))];
            Assert.That(controllers, Is.Not.Empty);

            foreach (Type controller in controllers)
            {
                Assert.That(controller.Namespace, Is.EqualTo("NuciCraft.API.Controllers"), controller.FullName);
                Assert.That(controller.IsSubclassOf(typeof(NuciApiController)), controller.FullName);
                Assert.That(controller.GetCustomAttribute<ApiControllerAttribute>(), Is.Not.Null, controller.FullName);
                Assert.That(controller.GetCustomAttributes<RouteAttribute>(), Is.Not.Empty, controller.FullName);
            }
        }

        [Test]
        public void GivenControllers_WhenCheckingInjectedServices_ThenOnlyServiceInterfacesAreInjected()
        {
            Type[] controllers = [.. ProductionArchitecture.Assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => string.Equals(type.Namespace, "NuciCraft.API.Controllers", StringComparison.Ordinal))
                .Where(type => type.IsPublic && type.IsClass)];
            Assert.That(controllers, Is.Not.Empty);

            foreach (Type controller in controllers)
            {
                ParameterInfo[] serviceParameters = [.. controller.GetConstructors()
                    .SelectMany(constructor => constructor.GetParameters())
                    .Where(parameter => parameter.ParameterType.Namespace?.StartsWith(
                        "NuciCraft.API.Service", StringComparison.Ordinal) is true)];
                Assert.That(serviceParameters, Is.Not.Empty, $"{controller.FullName} must delegate to a service.");
                Assert.That(
                    serviceParameters.Where(parameter => !parameter.ParameterType.IsInterface),
                    Is.Empty,
                    $"{controller.FullName} must inject service interfaces rather than implementations.");
            }
        }
    }
}