using NUnit.Framework;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class PlayerSettingsDataObjectTests
    {
        [Test]
        public void GivenANewPlayerSettingsDataObject_WhenReadingTheBooleanProperties_ThenTheyAreFalseByDefault()
        {
            PlayerSettingsDataObject playerSettingsDataObject = new();

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled, Is.False);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.KeepExperinceIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled, Is.False);
        }

        [Test]
        public void GivenConfiguredPlayerSettingsDataObject_WhenReadingTheBooleanProperties_ThenTheAssignedValuesAreReturned()
        {
            PlayerSettingsDataObject playerSettingsDataObject = new()
            {
                AutomaticSaplingReplantingIsEnabled = true,
                PrivateMessagesAreEnabled = true,
                PrivateMessagesInterceptionIsEnabled = true,
                AutomaticHotbarRefillingIsEnabled = true,
                KeepInventoryIsEnabled = true,
                KeepExperinceIsEnabled = true,
                AutomaticToolSelectionIsEnabled = true
            };

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled);
            Assert.That(playerSettingsDataObject.KeepExperinceIsEnabled);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled);
        }
    }
}