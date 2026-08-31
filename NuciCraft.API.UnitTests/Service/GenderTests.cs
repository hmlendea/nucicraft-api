using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class GenderTests
    {
        [TestCase("male")]
        [TestCase("MALE")]
        public void GivenAMaleExternalName_WhenParsingGender_ThenMaleIsReturned(string externalName)
            => Assert.That(Gender.FromString(externalName), Is.EqualTo(Gender.Male));

        [TestCase("female")]
        [TestCase("FEMALE")]
        public void GivenAFemaleExternalName_WhenParsingGender_ThenFemaleIsReturned(string externalName)
            => Assert.That(Gender.FromString(externalName), Is.EqualTo(Gender.Female));

        [TestCase("other")]
        [TestCase("OTHER")]
        public void GivenAnOtherExternalName_WhenParsingGender_ThenOtherIsReturned(string externalName)
            => Assert.That(Gender.FromString(externalName), Is.EqualTo(Gender.Other));

        [TestCase(null)]
        [TestCase("")]
        public void GivenNoExternalName_WhenParsingGender_ThenOtherIsReturned(string externalName)
            => Assert.That(Gender.FromString(externalName), Is.EqualTo(Gender.Other));

        [TestCase("unknown")]
        public void GivenAnUnknownExternalName_WhenParsingGender_ThenOtherIsReturned(string externalName)
            => Assert.That(Gender.FromString(externalName), Is.EqualTo(Gender.Other));
    }
}