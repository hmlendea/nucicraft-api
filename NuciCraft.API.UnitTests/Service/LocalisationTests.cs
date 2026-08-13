using System;
using System.Reflection;

using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class LocalisationTests
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

        [Test]
        public void GivenAllLocalisations_WhenGettingValues_ThenEveryLocalisationIsReturned()
            => Assert.That(
                Localisation.GetValues(),
                Has.Length.EqualTo(3));

        [Test]
        public void GivenNull_WhenComparingTypedLocalisations_ThenFalseIsReturned()
            => Assert.That(
                Localisation.English.Equals((Localisation)null),
                Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingTypedLocalisations_ThenTrueIsReturned()
            => Assert.That(Localisation.English.Equals(Localisation.English));

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingTypedLocalisations_ThenTrueIsReturned()
        {
            Localisation localisation = BuildLocalisation(
                nameof(Localisation.English),
                "english");

            Assert.That(Localisation.English.Equals(localisation));
        }

        [Test]
        public void GivenMatchingNamesAndDifferentExternalNames_WhenComparingTypedLocalisations_ThenFalseIsReturned()
        {
            Localisation localisation = BuildLocalisation(
                nameof(Localisation.English),
                "romanian");

            Assert.That(Localisation.English.Equals(localisation), Is.False);
        }

        [Test]
        public void GivenDifferentLocalisations_WhenComparingTypedLocalisations_ThenFalseIsReturned()
            => Assert.That(
                Localisation.English.Equals(Localisation.Romanian),
                Is.False);

        [Test]
        public void GivenNull_WhenComparingObjectLocalisations_ThenFalseIsReturned()
            => Assert.That(
                Localisation.English.Equals((object)null),
                Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingObjectLocalisations_ThenTrueIsReturned()
            => Assert.That(Localisation.English.Equals((object)Localisation.English));

        [Test]
        public void GivenAnotherType_WhenComparingObjectLocalisations_ThenFalseIsReturned()
            => Assert.That(
                Localisation.English.Equals("english"),
                Is.False);

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingObjectLocalisations_ThenTrueIsReturned()
        {
            object localisation = BuildLocalisation(
                nameof(Localisation.English),
                "english");

            Assert.That(Localisation.English.Equals(localisation));
        }

        [Test]
        public void GivenAnEquivalentLocalisation_WhenGettingHashCodes_ThenTheHashCodesAreEqual()
        {
            Localisation localisation = BuildLocalisation(
                nameof(Localisation.English),
                "english");

            Assert.That(
                Localisation.English.GetHashCode(),
                Is.EqualTo(localisation.GetHashCode()));
        }

        [Test]
        public void GivenALocalisation_WhenCallingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                Localisation.English.ToString(),
                Is.EqualTo("english"));

        [Test]
        public void GivenTwoNullLocalisations_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            Localisation current = null;
            Localisation other = null;

            Assert.That(current == other);
        }

        [Test]
        public void GivenNullAndNonNullLocalisations_WhenUsingTheEqualityOperator_ThenFalseIsReturned()
        {
            Localisation current = null;

            Assert.That(current == Localisation.English, Is.False);
        }

        [Test]
        public void GivenEquivalentLocalisations_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            Localisation localisation = BuildLocalisation(
                nameof(Localisation.English),
                "english");

            Assert.That(Localisation.English == localisation);
        }

        [Test]
        public void GivenDifferentLocalisations_WhenUsingTheInequalityOperator_ThenTrueIsReturned()
            => Assert.That(Localisation.English != Localisation.Romanian);

        private static Localisation BuildLocalisation(
            string name,
            string externalName)
            => Activator.CreateInstance(
                typeof(Localisation),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [name, externalName],
                null) as Localisation;
    }
}
