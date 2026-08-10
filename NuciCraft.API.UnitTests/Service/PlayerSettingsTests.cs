using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class PlayerSettingsTests
    {
        [Test]
        public void GivenANewPlayerSettings_WhenReadingTheBooleanProperties_ThenTheyAreFalseByDefault()
        {
            PlayerSettings playerSettings = new();

            Assert.That(playerSettings.AutomaticSaplingReplantingIsEnabled, Is.False);
            Assert.That(playerSettings.PrivateMessagesAreEnabled, Is.False);
            Assert.That(playerSettings.PrivateMessagesInterceptionIsEnabled, Is.False);
            Assert.That(playerSettings.AutomaticHotbarRefillingIsEnabled, Is.False);
            Assert.That(playerSettings.KeepInventoryIsEnabled, Is.False);
            Assert.That(playerSettings.KeepExperinceIsEnabled, Is.False);
            Assert.That(playerSettings.AutomaticToolSelectionIsEnabled, Is.False);
        }

        [Test]
        public void GivenConfiguredPlayerSettings_WhenReadingTheBooleanProperties_ThenTheAssignedValuesAreReturned()
        {
            PlayerSettings playerSettings = new()
            {
                AutomaticSaplingReplantingIsEnabled = true,
                PrivateMessagesAreEnabled = true,
                PrivateMessagesInterceptionIsEnabled = true,
                AutomaticHotbarRefillingIsEnabled = true,
                KeepInventoryIsEnabled = true,
                KeepExperinceIsEnabled = true,
                AutomaticToolSelectionIsEnabled = true
            };

            Assert.That(playerSettings.AutomaticSaplingReplantingIsEnabled);
            Assert.That(playerSettings.PrivateMessagesAreEnabled);
            Assert.That(playerSettings.PrivateMessagesInterceptionIsEnabled);
            Assert.That(playerSettings.AutomaticHotbarRefillingIsEnabled);
            Assert.That(playerSettings.KeepInventoryIsEnabled);
            Assert.That(playerSettings.KeepExperinceIsEnabled);
            Assert.That(playerSettings.AutomaticToolSelectionIsEnabled);
        }
    }
}