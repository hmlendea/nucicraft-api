using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class SourceFileConventionTests
    {
        public static IEnumerable<TestCaseData> SourceFiles => SourceProject.Files();

        public static IEnumerable<TestCaseData> ProductionFiles
            => SourceProject.Files().Where(testCase =>
                testCase.Arguments[0] is SourceProject { Name: "NuciCraft.API" });

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingDeclarations_ThenExactlyOneMatchingTypeExists(
            object sourceProject,
            object sourceTree)
        {
            SyntaxTree tree = (SyntaxTree)sourceTree;
            MemberDeclarationSyntax[] types = [.. tree.GetRoot().DescendantNodes()
                .OfType<MemberDeclarationSyntax>()
                .Where(member => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)];
            Assert.That(types, Has.Length.EqualTo(1), $"{tree.FilePath}: Declare exactly one type, including nested types.");
            Assert.That(
                SourceProject.TypeName(types.Single()),
                Is.EqualTo(Path.GetFileNameWithoutExtension(tree.FilePath)),
                $"{tree.FilePath}: Filename and type identifier must match case-sensitively.");
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingSyntax_ThenForbiddenCodingFormsAreAbsent(
            object sourceProject,
            object sourceTree)
        {
            SyntaxTree tree = (SyntaxTree)sourceTree;
            SyntaxNode root = tree.GetRoot();
            List<string> violations = [];

            foreach (SyntaxNode node in root.DescendantNodes(descendIntoTrivia: true))
            {
                string reason = ForbiddenForm(node);

                if (!string.IsNullOrEmpty(reason))
                {
                    violations.Add($"{SourceProject.Location(node)}: {reason}");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingNamespaces_ThenPlacementMatchesItsProjectAndFolder(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;
            CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();
            BaseNamespaceDeclarationSyntax[] namespaces = [.. root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>()];
            string relativeFolder = Path.GetRelativePath(project.DirectoryPath, Path.GetDirectoryName(tree.FilePath)!);
            string expectedNamespace = project.Name;

            if (!string.Equals(relativeFolder, ".", StringComparison.Ordinal))
            {
                expectedNamespace += "." + relativeFolder.Replace(Path.DirectorySeparatorChar, '.');
            }

            Assert.That(namespaces, Has.Length.EqualTo(1), $"{tree.FilePath}: One explicit namespace is required.");
            Assert.That(namespaces.Single(), Is.TypeOf<NamespaceDeclarationSyntax>(), tree.FilePath);
            Assert.That(namespaces.Single().Name.ToString(), Is.EqualTo(expectedNamespace), tree.FilePath);
            Assert.That(namespaces.Single().Usings, Is.Empty, $"{tree.FilePath}: Imports belong outside the namespace.");
            Assert.That(
                expectedNamespace.Split('.').Intersect(new[] { "Interfaces", "Enums", "Enumerations" }),
                Is.Empty,
                $"{tree.FilePath}: Interfaces and enumerations must be colocated with their domain.");
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingMembers_ThenAccessibilityAndParameterLimitsAreExplicit(
            object sourceProject,
            object sourceTree)
        {
            SyntaxTree tree = (SyntaxTree)sourceTree;
            List<string> violations = [];

            foreach (MemberDeclarationSyntax member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
            {
                if (RequiresAccessibility(member) && !member.Modifiers.Any(IsAccessibility))
                {
                    violations.Add($"{SourceProject.Location(member)}: Declare member accessibility explicitly.");
                }
            }

            foreach (ParameterListSyntax parameters in tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>())
            {
                if (parameters.Parameters.Count > 7)
                {
                    violations.Add($"{SourceProject.Location(parameters)}: At most seven parameters are permitted.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingWhitespace_ThenPortableFormattingIsUsed(
            object sourceProject,
            object sourceTree)
        {
            SyntaxTree tree = (SyntaxTree)sourceTree;
            string text = tree.GetText().ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
            string[] lines = text.Split('\n');
            List<string> violations = [];

            if (!text.EndsWith('\n'))
            {
                violations.Add($"{tree.FilePath}: End the file with a newline.");
            }

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex += 1)
            {
                string line = lines[lineIndex];

                if (line.TakeWhile(char.IsWhiteSpace).Contains('\t'))
                {
                    violations.Add($"{tree.FilePath}:{lineIndex + 1}: Use spaces, not indentation tabs.");
                }

                if (lineIndex > 0 && string.IsNullOrWhiteSpace(lines[lineIndex - 1]) &&
                    (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('}')))
                {
                    violations.Add($"{tree.FilePath}:{lineIndex + 1}: Remove the redundant blank line.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static string ForbiddenForm(SyntaxNode node)
            => node switch
            {
                VariableDeclarationSyntax declaration when declaration.Type.IsVar => "Use explicit variable types.",
                ConditionalExpressionSyntax => "Replace the ternary with explicit control flow.",
                TupleTypeSyntax or TupleExpressionSyntax => "Use a dedicated type instead of a tuple.",
                GlobalStatementSyntax => "Top-level statements are forbidden; declare Program.Main explicitly.",
                FileScopedNamespaceDeclarationSyntax => "Use a block namespace.",
                RegionDirectiveTriviaSyntax or EndRegionDirectiveTriviaSyntax => "Regions are forbidden.",
                NullableDirectiveTriviaSyntax directive when directive.SettingToken.IsKind(SyntaxKind.EnableKeyword)
                    => "Enable nullable references in project metadata, not a source directive.",
                ParameterSyntax parameter when parameter.Default is not null => "Use overloads instead of optional parameters.",
                ParameterSyntax parameter when parameter.Modifiers.Any(SyntaxKind.RefKeyword) => "Ref parameters are forbidden.",
                AccessorDeclarationSyntax accessor when accessor.IsKind(SyntaxKind.InitAccessorDeclaration)
                    => "Use set instead of init.",
                BinaryExpressionSyntax expression when expression.IsKind(SyntaxKind.CoalesceExpression)
                    => "Null coalescing is forbidden; null-coalescing assignment remains permitted.",
                BinaryExpressionSyntax expression when IsNullComparison(expression) => "Use is null or is not null.",
                ThrowStatementSyntax statement when IsCaughtException(statement) => "Use a bare rethrow to preserve the stack trace.",
                FieldDeclarationSyntax field when field.Modifiers.Any(SyntaxKind.ConstKeyword)
                    => "Represent constants with static expression-bodied properties.",
                _ => string.Empty
            };

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingControlFlow_ThenBodiesAreBracedAndSimpleMethodsUseExpressions(
            object sourceProject,
            object sourceTree)
        {
            SyntaxTree tree = (SyntaxTree)sourceTree;
            List<string> violations = [];

            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                StatementSyntax? body = node switch
                {
                    IfStatementSyntax statement => statement.Statement,
                    ElseClauseSyntax clause when clause.Statement is not IfStatementSyntax => clause.Statement,
                    ForStatementSyntax statement => statement.Statement,
                    CommonForEachStatementSyntax statement => statement.Statement,
                    WhileStatementSyntax statement => statement.Statement,
                    DoStatementSyntax statement => statement.Statement,
                    UsingStatementSyntax statement => statement.Statement,
                    LockStatementSyntax statement => statement.Statement,
                    _ => null
                };

                if (body is not null && body is not BlockSyntax)
                {
                    violations.Add($"{SourceProject.Location(node)}: Control-flow bodies require braces.");
                }

                if (node is MethodDeclarationSyntax { Body.Statements.Count: 1 } method &&
                    method.Body.Statements.Single() is ReturnStatementSyntax or ThrowStatementSyntax or ExpressionStatementSyntax)
                {
                    violations.Add($"{SourceProject.Location(method)}: Use an expression-bodied single-statement method.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingSemanticCalls_ThenDateFormattingAndLoggingUseRequiredContracts(
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

                string declaringType = method.ContainingType.ToDisplayString();

                if (declaringType is "System.DateTime" or "System.DateTimeOffset" &&
                    method.Name is "Parse" or "TryParse" or "ParseExact" or "TryParseExact" or "ToString" &&
                    !method.Parameters.Any(parameter =>
                        string.Equals(parameter.Type.ToDisplayString(), "System.IFormatProvider", StringComparison.Ordinal) ||
                        string.Equals(parameter.Name, "format", StringComparison.Ordinal)))
                {
                    violations.Add($"{SourceProject.Location(invocation)}: Date operations require a format or provider.");
                }

                if (method.ContainingNamespace.ToDisplayString().StartsWith("NuciLog", StringComparison.Ordinal) &&
                    method.Name is "Debug" or "Info" or "Warn" or "Error" or "Fatal" &&
                    !method.Parameters.Any(parameter => string.Equals(parameter.Type.Name, "Operation", StringComparison.Ordinal)))
                {
                    violations.Add($"{SourceProject.Location(invocation)}: NuciLog calls require an Operation argument.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(SourceFiles))]
        public void GivenASourceFile_WhenCheckingConstruction_ThenTypedInitialisersAndMutableAccessorsAreUsed(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;
            SemanticModel model = project.Compilation.GetSemanticModel(tree);
            List<string> violations = [];

            foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                if (creation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variable } &&
                    variable.Parent is VariableDeclarationSyntax declaration && !declaration.Type.IsVar &&
                    SymbolEqualityComparer.Default.Equals(model.GetTypeInfo(declaration.Type).Type, model.GetTypeInfo(creation).Type))
                {
                    violations.Add($"{SourceProject.Location(creation)}: Use target-typed new when the declaration specifies the type.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCaseSource(nameof(ProductionFiles))]
        public void GivenASourceFile_WhenCheckingRolePlacement_ThenContractsAndMappingsUseTheirPrescribedNamespaces(
            object sourceProject,
            object sourceTree)
        {
            SourceProject project = (SourceProject)sourceProject;
            SyntaxTree tree = (SyntaxTree)sourceTree;

            SemanticModel model = project.Compilation.GetSemanticModel(tree);
            List<string> violations = [];

            foreach (BaseTypeDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                INamedTypeSymbol? symbol = model.GetDeclaredSymbol(declaration);
                Assert.That(symbol, Is.Not.Null, $"{SourceProject.Location(declaration)}: Type symbol must resolve.");
                string name = declaration.Identifier.ValueText;
                string actualNamespace = symbol!.ContainingNamespace.ToDisplayString();
                string expectedNamespace = ExpectedNamespace(name);

                if (!string.IsNullOrEmpty(expectedNamespace) &&
                    !string.Equals(actualNamespace, expectedNamespace, StringComparison.Ordinal))
                {
                    violations.Add($"{SourceProject.Location(declaration)}: {name} belongs in {expectedNamespace}.");
                }

                if (declaration is InterfaceDeclarationSyntax &&
                    (name.Length < 2 || !name.StartsWith('I') || !char.IsUpper(name[1])))
                {
                    violations.Add($"{SourceProject.Location(declaration)}: Interfaces require I-prefixed PascalCase names.");
                }

                if (name.EndsWith("MappingExtensions", StringComparison.Ordinal) &&
                    (!symbol.IsStatic || symbol.DeclaredAccessibility is not Accessibility.Internal))
                {
                    violations.Add($"{SourceProject.Location(declaration)}: Mapping extensions must be internal static classes.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static string ExpectedNamespace(string name)
        {
            foreach (string suffix in new[] { "Controller", "Request", "Response", "Settings", "DataObject", "MappingExtensions" })
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return suffix switch
                    {
                        "Controller" => "NuciCraft.API.Controllers",
                        "Request" => "NuciCraft.API.Requests",
                        "Response" => "NuciCraft.API.Responses",
                        "Settings" => "NuciCraft.API.Configuration",
                        "DataObject" => "NuciCraft.API.DataAccess.DataObjects",
                        "MappingExtensions" => "NuciCraft.API.Service.Mapping",
                        _ => string.Empty
                    };
                }
            }

            return string.Empty;
        }

        private static bool IsNullComparison(BinaryExpressionSyntax expression)
            => (expression.IsKind(SyntaxKind.EqualsExpression) || expression.IsKind(SyntaxKind.NotEqualsExpression)) &&
                (expression.Left.IsKind(SyntaxKind.NullLiteralExpression) ||
                    expression.Right.IsKind(SyntaxKind.NullLiteralExpression));

        private static bool IsCaughtException(ThrowStatementSyntax statement)
            => statement.Expression is IdentifierNameSyntax identifier &&
                statement.Ancestors().OfType<CatchClauseSyntax>().Any(clause =>
                    string.Equals(clause.Declaration?.Identifier.ValueText, identifier.Identifier.ValueText, StringComparison.Ordinal));

        private static bool RequiresAccessibility(MemberDeclarationSyntax member)
            => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or BaseFieldDeclarationSyntax or
                BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax &&
                member is not DestructorDeclarationSyntax &&
                member.Parent is not InterfaceDeclarationSyntax &&
                member is not MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null } &&
                member is not PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null };

        private static bool IsAccessibility(SyntaxToken modifier)
            => modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword) || modifier.IsKind(SyntaxKind.PrivateKeyword);
    }
}