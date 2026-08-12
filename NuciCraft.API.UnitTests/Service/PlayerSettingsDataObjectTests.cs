using NUnit.Framework;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class PlayerSettingsDataObjectTests
    {
        [Test]
        public void GivenANewPlayerSettingsDataObject_WhenReadingTheBooleanProperties_ThenTheyAreNullByDefault()
        {
            PlayerSettingsDataObject playerSettingsDataObject = new();

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.KeepExperienceIsEnabled, Is.Null);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled, Is.Null);
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
                KeepExperienceIsEnabled = true,
                AutomaticToolSelectionIsEnabled = true
            };

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled, Is.True);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.KeepExperienceIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled, Is.True);
        }
    }
}