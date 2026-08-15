using System.Reflection;

using NUnit.Framework;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class PlayerSettingsDataObjectTests
    {
        [Test]
        public void GivenANewPlayerSettingsDataObject_WhenReadingTheBooleanProperties_ThenTheExpectedDefaultsAreReturned()
        {
            PlayerSettingsDataObject playerSettingsDataObject = new();

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.KeepExperienceIsEnabled, Is.False);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled);
            Assert.That(playerSettingsDataObject.TeleportationRequestsAreEnabled);
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
                AutomaticToolSelectionIsEnabled = true,
                TeleportationRequestsAreEnabled = false
            };

            Assert.That(playerSettingsDataObject.AutomaticSaplingReplantingIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.PrivateMessagesAreEnabled, Is.True);
            Assert.That(playerSettingsDataObject.PrivateMessagesInterceptionIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.AutomaticHotbarRefillingIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.KeepInventoryIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.KeepExperienceIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.AutomaticToolSelectionIsEnabled, Is.True);
            Assert.That(playerSettingsDataObject.TeleportationRequestsAreEnabled, Is.False);
        }

        [Test]
        public void GivenTheTeleportationRequestsSetting_WhenInspectingItsContract_ThenItHasTheNextHmacOrder()
        {
            PropertyInfo settingProperty = typeof(PlayerSettingsDataObject)
                .GetProperty(nameof(PlayerSettingsDataObject.TeleportationRequestsAreEnabled));
            HmacOrderAttribute hmacOrderAttribute = settingProperty
                .GetCustomAttribute<HmacOrderAttribute>();

            Assert.That(hmacOrderAttribute, Is.Not.Null);
            Assert.That(hmacOrderAttribute.Order, Is.EqualTo(10));
        }

        [TestCase(nameof(PlayerSettingsDataObject.AutomaticHotbarRefillingIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.AutomaticSaplingReplantingIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.AutomaticToolSelectionIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.KeepExperienceIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.KeepInventoryIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.PrivateMessagesAreEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.PrivateMessagesInterceptionIsEnabled))]
        [TestCase(nameof(PlayerSettingsDataObject.TeleportationRequestsAreEnabled))]
        public void GivenABooleanPlayerSetting_WhenInspectingItsType_ThenItIsNonNullable(
            string settingPropertyName)
        {
            PropertyInfo settingProperty = typeof(PlayerSettingsDataObject)
                .GetProperty(settingPropertyName);

            Assert.That(settingProperty.PropertyType, Is.EqualTo(typeof(bool)));
        }
    }
}
