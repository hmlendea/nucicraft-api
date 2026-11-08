using NUnit.Framework;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.UnitTests.Configuration
{
    [TestFixture]
    public sealed class AnonymisationServiceDetectionSettingsTests
    {
        [Test]
        public void GivenDefaultSettings_WhenReadingTheCacheDurations_ThenTheyMatchTheDocumentedValues()
        {
            AnonymisationServiceDetectionSettings settings = new();

            Assert.That(settings.CacheDurationHoursWhenDetected, Is.EqualTo(6));
            Assert.That(settings.CacheDurationHoursWhenNotDetected, Is.EqualTo(2));
        }

        [Test]
        public void GivenDefaultSettings_WhenReadingTheProviderAndFlags_ThenTheyMatchTheDocumentedValues()
        {
            AnonymisationServiceDetectionSettings settings = new();

            Assert.That(settings.Provider, Is.EqualTo("proxycheck"));
            Assert.That(settings.IsEnabled, Is.True);
            Assert.That(settings.IsCacheEnabled, Is.True);
        }
    }
}
