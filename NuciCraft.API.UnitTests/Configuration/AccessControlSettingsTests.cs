using NUnit.Framework;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.UnitTests.Configuration
{
    [TestFixture]
    public sealed class AccessControlSettingsTests
    {
        [Test]
        public void GivenDefaultSettings_WhenReadingTheRegisteredPlayerExemptionFlag_ThenItIsEnabled()
        {
            AccessControlSettings settings = new();

            Assert.That(settings.IsRegisteredPlayerExemptionEnabled, Is.True);
        }

        [Test]
        public void GivenDefaultSettings_WhenReadingTheMinimumUsernameLength_ThenItMatchesTheDocumentedValue()
        {
            AccessControlSettings settings = new();

            Assert.That(settings.MinimumUsernameLength, Is.EqualTo(4));
        }
    }
}
