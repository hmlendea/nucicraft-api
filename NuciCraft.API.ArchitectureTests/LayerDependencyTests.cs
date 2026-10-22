using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace NuciCraft.API.ArchitectureTests
{
    [TestFixture]
    public sealed class LayerDependencyTests
    {
        [TestCase("Service", "Controllers")]
        [TestCase("Service", "Responses")]
        [TestCase("Service.Models", "DataAccess")]
        [TestCase("Service.Models", "Requests")]
        [TestCase("Service.Models", "Responses")]
        [TestCase("Service.Models", "Controllers")]
        [TestCase("Service.Models", "Configuration")]
        [TestCase("DataAccess", "Controllers")]
        [TestCase("DataAccess", "Responses")]
        [TestCase("DataAccess", "Service")]
        [TestCase("Responses", "DataAccess")]
        [TestCase("Responses", "Controllers")]
        [TestCase("Configuration", "Controllers")]
        [TestCase("Configuration", "Service")]
        [TestCase("Configuration", "DataAccess")]
        [TestCase("Logging", "Controllers")]
        [TestCase("Logging", "Service")]
        [TestCase("Logging", "DataAccess")]
        public void GivenProductionLayers_WhenCheckingDependencies_ThenReverseDependenciesAreForbidden(
            string sourceNamespace,
            string targetNamespace)
        {
            IObjectProvider<IType> source = SelectLayer(sourceNamespace);
            IObjectProvider<IType> target = SelectLayer(targetNamespace);
            Assert.That(source.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);
            Assert.That(target.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(source).Should().NotDependOnAny(target)
                .Because($"{sourceNamespace} must not depend on {targetNamespace}.")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenControllers_WhenCheckingDependencies_ThenServicesAreAccessedThroughInterfaces()
        {
            IObjectProvider<IType> controllers = SelectLayer("Controllers");
            IObjectProvider<Class> implementations = Classes().That()
                .ResideInNamespace("NuciCraft.API.Service");
            Assert.That(controllers.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);
            Assert.That(implementations.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(controllers).Should().NotDependOnAny(implementations)
                .Because("Controllers delegate application logic through service interfaces.")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenControllers_WhenCheckingPersistenceDependencies_ThenOnlyZoneCoordinatesArePermitted()
        {
            IObjectProvider<IType> controllers = Types().That().Are(SelectLayer("Controllers"))
                .And().DoNotHaveFullName("NuciCraft.API.Controllers.ZonesController");
            Assert.That(controllers.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(controllers).Should().NotDependOnAny(SelectLayer("DataAccess"))
                .Because("Only ZonesController currently constructs persistence-owned coordinate contracts.")
                .Check(ProductionArchitecture.Model);

            IObjectProvider<IType> zoneController = Types().That()
                .HaveFullName("NuciCraft.API.Controllers.ZonesController");
            Assert.That(zoneController.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(zoneController).Should().NotDependOnAny(
                Types().That().Are(SelectLayer("DataAccess"))
                    .And().DoNotHaveFullName("NuciCraft.API.DataAccess.DataObjects.CoordinatesDataObject"))
                .Because("The legacy exception permits coordinates only, not other persistence types.")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenPersistence_WhenCheckingRequestDependencies_ThenOnlyPlayerSettingsMergeIsPermitted()
        {
            IObjectProvider<IType> records = Types().That().Are(SelectLayer("DataAccess"))
                .And().DoNotHaveFullName(
                    "NuciCraft.API.DataAccess.DataObjects.PlayerSettingsDataObjectExtensions");
            Assert.That(records.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(records).Should().NotDependOnAny(SelectLayer("Requests"))
                .Because("Only the legacy player-settings merge adapter consumes a patch request.")
                .Check(ProductionArchitecture.Model);

            IObjectProvider<IType> mergeAdapter = Types().That().HaveFullName(
                "NuciCraft.API.DataAccess.DataObjects.PlayerSettingsDataObjectExtensions");
            Assert.That(mergeAdapter.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(mergeAdapter).Should().NotDependOnAny(
                Types().That().Are(SelectLayer("Requests"))
                    .And().DoNotHaveFullName("NuciCraft.API.Requests.PatchPlayerSettingsRequest"))
                .Because("The merge adapter may depend only on the player-settings patch contract.")
                .Check(ProductionArchitecture.Model);
        }

        [TestCase("Controllers", @"^NuciDAL(\.|$)")]
        [TestCase("Controllers", @"^NuciAPI\.Client(\.|$)")]
        [TestCase("Controllers", @"^NuciCraft\.API\.Service\.(Helpers|Mapping|Mappings)(\.|$)")]
        [TestCase("Service", @"^Microsoft\.AspNetCore\.Mvc(\.|$)")]
        [TestCase("Controllers", @"^(Microsoft\.EntityFrameworkCore|Amazon\.DynamoDBv2)(\.|$)")]
        [TestCase("Service", @"^(Microsoft\.EntityFrameworkCore|Amazon\.DynamoDBv2)(\.|$)")]
        public void GivenBoundaryTypes_WhenCheckingDependencies_ThenInfrastructureLeakageIsForbidden(
            string sourceNamespace,
            string forbiddenNamespacePattern)
        {
            IObjectProvider<IType> source = SelectLayer(sourceNamespace);
            Assert.That(source.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);

            Types().That().Are(source).Should().NotDependOnAny(
                Types().That().ResideInNamespaceMatching(forbiddenNamespacePattern))
                .Because($"{sourceNamespace} must not depend on {forbiddenNamespacePattern}.")
                .Check(ProductionArchitecture.Model);
        }

        private static IObjectProvider<IType> SelectLayer(string relativeNamespace)
            => Types().That().ResideInNamespaceMatching(
                "^NuciCraft\\.API\\." + relativeNamespace.Replace(".", "\\.") + "(\\.|$)");
    }
}