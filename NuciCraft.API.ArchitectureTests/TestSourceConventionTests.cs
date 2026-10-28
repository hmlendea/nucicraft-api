using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class TestSourceConventionTests
    {
        public static IEnumerable<TestCaseData> TestFiles
            => SourceProject.Files().Where(testCase => testCase.Arguments[0] is SourceProject project &&
                !string.Equals(project.Name, "NuciCraft.API", StringComparison.Ordinal));

        [TestCaseSource(nameof(TestFiles))]
        public void GivenTestSources_WhenCheckingFixtures_ThenNUnitMetadataAndSubjectNamesAreExplicit(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;
            SemanticModel model = project.Compilation.GetSemanticModel(tree);
            List<string> violations = [];

            foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                INamedTypeSymbol? symbol = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
                Assert.That(symbol, Is.Not.Null, SourceProject.Location(declaration));
                IMethodSymbol[] tests = [.. symbol!.GetMembers().OfType<IMethodSymbol>().Where(IsTest)];

                if (tests.Length < 1)
                {
                    continue;
                }

                if (!symbol.Name.EndsWith("Tests", StringComparison.Ordinal) ||
                    (!HasAttribute(symbol, "NUnit.Framework.TestFixtureAttribute") &&
                        !string.Equals(symbol.Name, "SignIdGeneratorTests", StringComparison.Ordinal)))
                {
                    violations.Add($"{SourceProject.Location(declaration)}: NUnit fixtures require a subject-based Tests name and TestFixture.");
                }

                foreach (IMethodSymbol test in tests)
                {
                    CheckTestMethod(project, test, violations);
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(TestFiles))]
        public void GivenTestSources_WhenCheckingAssertions_ThenNUnitUsesConstraintAssertions(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;
            SemanticModel model = project.Compilation.GetSemanticModel(tree);
            List<string> violations = [];

            foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                IMethodSymbol? method = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                if (method is null)
                {
                    continue;
                }

                string typeName = method.ContainingType.ToDisplayString();

                if (typeName is "NUnit.Framework.Legacy.ClassicAssert" ||
                    (typeName is "NUnit.Framework.Assert" &&
                        method.Name is "AreEqual" or "AreNotEqual" or "IsTrue" or "IsFalse" or "IsNull" or "IsNotNull"))
                {
                    violations.Add($"{SourceProject.Location(invocation)}: Use Assert.That with NUnit constraints.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(TestFiles))]
        public void GivenTestSources_WhenCheckingSerialisation_ThenOptionsAreCachedInStaticReadonlyFields(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;
            SemanticModel model = project.Compilation.GetSemanticModel(tree);
            List<string> violations = [];

            foreach (BaseObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
            {
                if (!string.Equals(model.GetTypeInfo(creation).Type?.ToDisplayString(), "System.Text.Json.JsonSerializerOptions", StringComparison.Ordinal))
                {
                    continue;
                }

                FieldDeclarationSyntax? field = creation.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault();
                IFieldSymbol? symbol = field?.Declaration.Variables.Select(variable => model.GetDeclaredSymbol(variable))
                    .OfType<IFieldSymbol>().FirstOrDefault();

                if (symbol is null || !symbol.IsStatic || !symbol.IsReadOnly)
                {
                    violations.Add($"{SourceProject.Location(creation)}: Cache serializer options in a static readonly field.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static void CheckTestMethod(SourceProject project, IMethodSymbol method, ICollection<string> violations)
        {
            string location = method.Locations.First().GetLineSpan().ToString();

            if (string.Equals(project.Name, "NuciCraft.API.UnitTests", StringComparison.Ordinal) &&
                !string.Equals(method.ContainingType.Name, "SignIdGeneratorTests", StringComparison.Ordinal) &&
                (!method.Name.StartsWith("Given", StringComparison.Ordinal) ||
                    !method.Name.Contains("_When", StringComparison.Ordinal) ||
                    !method.Name.Contains("_Then", StringComparison.Ordinal)))
            {
                violations.Add($"{location}: Unit-test names require Given..._When..._Then....");
            }

            int caseCount = method.GetAttributes().Count(attribute => string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "NUnit.Framework.TestCaseAttribute",
                StringComparison.Ordinal));

            if (caseCount is 1 && string.Equals(project.Name, "NuciCraft.API.UnitTests", StringComparison.Ordinal) &&
                !string.Equals(method.ContainingType.Name, "GenderTests", StringComparison.Ordinal))
            {
                violations.Add($"{location}: Parameterised unit tests require at least two cases.");
            }
        }

        private static bool IsTest(IMethodSymbol method)
            => HasAttribute(method, "NUnit.Framework.TestAttribute") ||
                HasAttribute(method, "NUnit.Framework.TestCaseAttribute") ||
                HasAttribute(method, "NUnit.Framework.TestCaseSourceAttribute");

        private static bool HasAttribute(ISymbol symbol, string attributeName)
            => symbol.GetAttributes().Any(attribute => string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                attributeName,
                StringComparison.Ordinal));
    }
}