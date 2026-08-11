using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class PlayerSettingsTests
    {
        [Test]
        public void GivenANewPlayerSettings_WhenReadingTheBooleanProperties_ThenTheExpectedDefaultValuesAreReturned()
        {
            PlayerSettings playerSettings = new();

            Assert.That(playerSettings.AutomaticHotbarRefillingIsEnabled);
            Assert.That(playerSettings.AutomaticSaplingReplantingIsEnabled);
            Assert.That(playerSettings.AutomaticToolSelectionIsEnabled, Is.False);
            Assert.That(playerSettings.KeepExperienceIsEnabled, Is.False);
            Assert.That(playerSettings.KeepInventoryIsEnabled, Is.False);
            Assert.That(playerSettings.PrivateMessagesAreEnabled);
            Assert.That(playerSettings.PrivateMessagesInterceptionIsEnabled, Is.False);
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
                KeepExperienceIsEnabled = true,
                AutomaticToolSelectionIsEnabled = true
            };

            Assert.That(playerSettings.AutomaticSaplingReplantingIsEnabled);
            Assert.That(playerSettings.PrivateMessagesAreEnabled);
            Assert.That(playerSettings.PrivateMessagesInterceptionIsEnabled);
            Assert.That(playerSettings.AutomaticHotbarRefillingIsEnabled);
            Assert.That(playerSettings.KeepInventoryIsEnabled);
            Assert.That(playerSettings.KeepExperienceIsEnabled);
            Assert.That(playerSettings.AutomaticToolSelectionIsEnabled);
        }
    }
}