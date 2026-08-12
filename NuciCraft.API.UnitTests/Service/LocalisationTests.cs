using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class LocalisationTests
    {
        [Test]
        public void GivenTheEnglishIdentifier_WhenConvertingFromString_ThenEnglishIsReturned()
        {
            Localisation localisation = Localisation.FromString("english");

            Assert.That(localisation, Is.EqualTo(Localisation.English));
        }

        [Test]
        public void GivenTheRomanianIdentifier_WhenConvertingFromString_ThenRomanianIsReturned()
        {
            Localisation localisation = Localisation.FromString("romanian");

            Assert.That(localisation, Is.EqualTo(Localisation.Romanian));
        }

        [Test]
        public void GivenAKnownIdentifierWithDifferentCasing_WhenConvertingFromString_ThenMatchingValueIsReturned()
        {
            Localisation localisation = Localisation.FromString("RoMaNiAn");

            Assert.That(localisation, Is.EqualTo(Localisation.Romanian));
        }

        [Test]
        public void GivenAnUnknownIdentifier_WhenConvertingFromString_ThenUnsupportedIsReturned()
        {
            Localisation localisation = Localisation.FromString("spanish");

            Assert.That(localisation, Is.EqualTo(Localisation.Unsupported));
        }

        [Test]
        public void GivenANullIdentifier_WhenConvertingFromString_ThenUnsupportedIsReturned()
        {
            Localisation localisation = Localisation.FromString(null);

            Assert.That(localisation, Is.EqualTo(Localisation.Unsupported));
        }

        [Test]
        public void GivenALocalisation_WhenConvertingToString_ThenTheIdentifierIsReturned()
        {
            string localisationIdentifier = Localisation.Romanian;

            Assert.That(localisationIdentifier, Is.EqualTo("romanian"));
        }
    }
}
