using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class ProductionReferenceTests
    {
        [Test]
        public void GivenProductionAssemblies_WhenCheckingReferences_ThenTestAndUnusedPersistenceFrameworksAreAbsent()
        {
            Assert.That(ProductionArchitecture.Assemblies, Is.Not.Empty);

            foreach (Assembly assembly in ProductionArchitecture.Assemblies)
            {
                Assert.That(
                    assembly.GetReferencedAssemblies().Select(reference => reference.Name)
                        .Where(name => name is not null && IsForbiddenReference(name)),
                    Is.Empty,
                    $"{assembly.GetName().Name} must not reference test code or unapproved persistence frameworks.");
            }
        }

        [Test]
        public void GivenProductionProjects_WhenCheckingDeclaredReferences_ThenTestAndUnusedPersistenceFrameworksAreAbsent()
        {
            string[] projectPaths = Directory.GetFiles(
                Path.Combine(TestContext.CurrentContext.TestDirectory, "ProductionProjects"),
                "*.csproj");
            Assert.That(projectPaths, Has.Length.EqualTo(ProductionArchitecture.Assemblies.Length));

            foreach (string projectPath in projectPaths)
            {
                XElement project = XElement.Load(projectPath);
                Assert.That(
                    project.Descendants("ProjectReference"),
                    Is.Empty,
                    $"{projectPath} is the single production project and must not depend on test projects.");
                Assert.That(
                    project.Descendants("PackageReference")
                        .Select(reference => (string?)reference.Attribute("Include"))
                        .Where(name => name is not null && IsForbiddenReference(name)),
                    Is.Empty,
                    $"{projectPath} must not declare test packages or unapproved persistence frameworks.");
                Assert.That(
                    project.Descendants().Where(element =>
                        string.Equals(element.Name.LocalName, "InternalsVisibleTo", StringComparison.Ordinal) ||
                        ((string?)element.Attribute("Include"))?.Contains(
                            nameof(InternalsVisibleToAttribute), StringComparison.Ordinal) is true),
                    Is.Empty,
                    $"{projectPath} must not declare friend assemblies for testing.");
            }
        }

        [Test]
        public void GivenProductionAssemblies_WhenCheckingVisibility_ThenNoFriendAssembliesAreDeclared()
        {
            foreach (Assembly assembly in ProductionArchitecture.Assemblies)
            {
                Assert.That(
                    assembly.GetCustomAttributes<InternalsVisibleToAttribute>(),
                    Is.Empty,
                    $"{assembly.GetName().Name} must not expose internals to other assemblies.");
            }
        }

        private static bool IsForbiddenReference(string name)
            => name.Contains("Tests", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("NUnit", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Moq", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("AWSSDK.DynamoDB", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Amazon.DynamoDBv2", StringComparison.OrdinalIgnoreCase);
    }
}