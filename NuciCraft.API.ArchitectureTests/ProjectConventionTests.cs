using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        public static IEnumerable<TestCaseData> Projects
            => SourceProject.All.Select(project => new TestCaseData(project.Name));

        [TestCaseSource(nameof(Projects))]
        public void GivenAProject_WhenCheckingDeclaredMetadata_ThenRequiredConventionsAreExplicit(string projectName)
        {
            XElement project = XElement.Load(ProjectPath(projectName));
            List<string> violations = [];
            CheckProperty(project, "TargetFramework", "net10.0", violations);
            CheckProperty(project, "ImplicitUsings", "disable", violations);
            CheckProperty(project, "LangVersion", "latest", violations);
            CheckProperty(project, "Nullable", "enable", violations);
            CheckProperty(project, "IsPackable", "false", violations);

            if (!string.Equals(projectName, "NuciCraft.API", StringComparison.Ordinal))
            {
                CheckProperty(project, "IsTestProject", "true", violations);
                CheckProperty(project, "ExcludeFromCodeCoverage", "true", violations);
            }

            string[] packages = [.. project.Descendants("PackageReference").Select(reference => (string)reference.Attribute("Include")!)];
            Assert.That(packages, Is.Ordered.Using<string>(StringComparer.Ordinal), $"{projectName}: Alphabetise packages.");
            Assert.That(violations, Is.Empty, $"{projectName}: {string.Join("; ", violations)}");
        }

        [TestCaseSource(nameof(Projects))]
        public void GivenAProject_WhenEvaluatingConfigurations_ThenImportsCannotWeakenRequiredSettings(string projectName)
        {
            foreach (string configuration in new[] { "Debug", "Release" })
            {
                using JsonDocument metadata = EvaluateProject(projectName, configuration);
                JsonElement properties = metadata.RootElement.GetProperty("Properties");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(properties.GetProperty("TargetFramework").GetString(), Is.EqualTo("net10.0"));
                    Assert.That(properties.GetProperty("ImplicitUsings").GetString(), Is.EqualTo("disable"));
                    Assert.That(properties.GetProperty("LangVersion").GetString(), Is.EqualTo("latest"));
                    Assert.That(properties.GetProperty("Nullable").GetString(), Is.EqualTo("enable"));
                    Assert.That(properties.GetProperty("IsPackable").GetString(), Is.EqualTo("false"));
                }

                string[] references = [.. metadata.RootElement.GetProperty("Items").GetProperty("ProjectReference")
                    .EnumerateArray().Select(reference => reference.GetProperty("FullPath").GetString()!)];

                if (string.Equals(projectName, "NuciCraft.API", StringComparison.Ordinal))
                {
                    Assert.That(references, Is.Empty, $"{projectName}/{configuration}: Production cannot reference tests.");
                }
                else
                {
                    Assert.That(references, Is.EquivalentTo(new[] { ProjectPath("NuciCraft.API") }));
                }
            }
        }

        [Test]
        public void GivenTheSolution_WhenCheckingStructure_ThenProjectsAreExplicitAndOrdered()
        {
            string path = Path.Combine(SourceProject.RepositoryRoot, "NuciCraft.API.slnx");
            Assert.That(File.Exists(path), $"Required solution is missing: {path}.");
            XDocument document = XDocument.Load(path);
            XElement solution = document.Root!;
            Assert.That(document.Declaration, Is.Null, path);
            Assert.That(solution.Name.LocalName, Is.EqualTo("Solution"), path);
            Assert.That(solution.Attributes(), Is.Empty, path);
            Assert.That(solution.Elements().Select(element => element.Name.LocalName), Is.All.EqualTo("Project"));
            Assert.That(Directory.GetFiles(SourceProject.RepositoryRoot, "*.sln"), Is.Empty);
            Assert.That(
                solution.Elements("Project").Select(element => (string?)element.Attribute("Path")),
                Is.EqualTo(SourceProject.All.Select(project => $"{project.Name}/{project.Name}.csproj")));

            foreach (XElement project in solution.Elements("Project"))
            {
                Assert.That(project.Attributes().Select(attribute => attribute.Name.LocalName), Is.EqualTo(new[] { "Path" }));
                Assert.That(project.Elements(), Is.Empty);
            }
        }

        [Test]
        public void GivenTheArchitectureProject_WhenCheckingMechanisms_ThenRequiredAnalysisPackagesArePresent()
        {
            XElement project = XElement.Load(ProjectPath("NuciCraft.API.ArchitectureTests"));
            string[] packages = [.. project.Descendants("PackageReference").Select(reference => (string)reference.Attribute("Include")!)];
            Assert.That(packages, Does.Contain("NUnit"));
            Assert.That(packages, Does.Contain("TngTech.ArchUnitNET.NUnit"));
            Assert.That(packages, Does.Contain("Microsoft.CodeAnalysis.CSharp"));
            Assert.That(ProductionArchitecture.Assemblies, Has.Length.EqualTo(1));
            Assert.That(ProductionArchitecture.Assemblies.Single().GetName().Name, Is.EqualTo("NuciCraft.API"));
        }

        private static string ProjectPath(string projectName)
            => Path.GetFullPath(Path.Combine(SourceProject.RepositoryRoot, projectName, $"{projectName}.csproj"));

        private static void CheckProperty(
            XElement project,
            string name,
            string expected,
            ICollection<string> violations)
        {
            string[] values = [.. project.Descendants(name).Select(property => property.Value)];

            if (values.Length < 1 || values.Any(value => !string.Equals(value, expected, StringComparison.Ordinal)))
            {
                violations.Add($"Declare {name}={expected} without contradictory values.");
            }
        }

        private static JsonDocument EvaluateProject(string projectName, string configuration)
        {
            ProcessStartInfo start = new("dotnet")
            {
                WorkingDirectory = SourceProject.RepositoryRoot,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add("msbuild");
            start.ArgumentList.Add(ProjectPath(projectName));
            start.ArgumentList.Add($"-property:Configuration={configuration}");
            start.ArgumentList.Add("-getProperty:TargetFramework,ImplicitUsings,LangVersion,Nullable,IsPackable");
            start.ArgumentList.Add("-getItem:ProjectReference,PackageReference");
            using Process process = new() { StartInfo = start };
            Assert.That(process.Start(), $"Cannot evaluate {projectName}/{configuration}.");
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Assert.That(process.ExitCode, Is.Zero, $"MSBuild evaluation failed for {projectName}: {output}");

            return JsonDocument.Parse(output);
        }
    }
}