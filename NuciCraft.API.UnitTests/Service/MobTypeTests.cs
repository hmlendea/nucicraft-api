using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class MobTypeTests
    {
        [Test]
        public void GivenAKnownExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("wandering_trader");

            Assert.That(mobType, Is.EqualTo(MobType.WanderingTrader));
        }

        [Test]
        public void GivenTheCowExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("cow");

            Assert.That(mobType, Is.EqualTo(MobType.Cow));
        }

        [Test]
        public void GivenThePigExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("pig");

            Assert.That(mobType, Is.EqualTo(MobType.Pig));
        }

        [Test]
        public void GivenTheEnderDragonExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("ender_dragon");

            Assert.That(mobType, Is.EqualTo(MobType.EnderDragon));
        }

        [TestCase("evoker", nameof(MobType.Evoker))]
        [TestCase("illusioner", nameof(MobType.Illusioner))]
        [TestCase("pillager", nameof(MobType.Pillager))]
        [TestCase("vindicator", nameof(MobType.Vindicator))]
        public void GivenTheNewIllagerExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned(
            string externalName,
            string expectedMobTypeName)
        {
            MobType mobType = MobType.FromString(externalName);

            Assert.That(mobType.Name, Is.EqualTo(expectedMobTypeName));
        }

        [Test]
        public void GivenTheVillageExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("villager");

            Assert.That(mobType, Is.EqualTo(MobType.Villager));
        }

        [Test]
        public void GivenAKnownExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("WANDERING_TRADER");

            Assert.That(mobType, Is.EqualTo(MobType.WanderingTrader));
        }

        [Test]
        public void GivenTheCowExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("COW");

            Assert.That(mobType, Is.EqualTo(MobType.Cow));
        }

        [Test]
        public void GivenThePigExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("PIG");

            Assert.That(mobType, Is.EqualTo(MobType.Pig));
        }

        [Test]
        public void GivenTheEnderDragonExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("ENDER_DRAGON");

            Assert.That(mobType, Is.EqualTo(MobType.EnderDragon));
        }

        [TestCase("EVOKER", nameof(MobType.Evoker))]
        [TestCase("ILLUSIONER", nameof(MobType.Illusioner))]
        [TestCase("PILLAGER", nameof(MobType.Pillager))]
        [TestCase("VINDICATOR", nameof(MobType.Vindicator))]
        public void GivenTheNewIllagerExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned(
            string externalName,
            string expectedMobTypeName)
        {
            MobType mobType = MobType.FromString(externalName);

            Assert.That(mobType.Name, Is.EqualTo(expectedMobTypeName));
        }

        [Test]
        public void GivenTheVillageExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("VILLAGER");

            Assert.That(mobType, Is.EqualTo(MobType.Villager));
        }

        [Test]
        public void GivenAnUnknownExternalName_WhenConvertingFromString_ThenUnsupportedIsReturned()
        {
            MobType mobType = MobType.FromString("zombie");

            Assert.That(mobType, Is.EqualTo(MobType.Unsupported));
        }

        [Test]
        public void GivenAMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.WanderingTrader,
                Is.EqualTo("wandering_trader"));

        [Test]
        public void GivenTheCowMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.Cow,
                Is.EqualTo("cow"));

        [Test]
        public void GivenThePigMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.Pig,
                Is.EqualTo("pig"));

        [Test]
        public void GivenTheEnderDragonMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.EnderDragon,
                Is.EqualTo("ender_dragon"));

        [TestCase(nameof(MobType.Evoker), "evoker")]
        [TestCase(nameof(MobType.Illusioner), "illusioner")]
        [TestCase(nameof(MobType.Pillager), "pillager")]
        [TestCase(nameof(MobType.Vindicator), "vindicator")]
        public void GivenTheNewIllagerMobType_WhenConvertingToString_ThenTheExternalNameIsReturned(
            string mobTypeName,
            string expectedExternalName)
        {
            MobType mobType = MobType.FromString(expectedExternalName);

            Assert.That((string)mobType, Is.EqualTo(expectedExternalName));
            Assert.That(mobType.Name, Is.EqualTo(mobTypeName));
        }

        [Test]
        public void GivenTheVillageMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.Villager,
                Is.EqualTo("villager"));
    }
}