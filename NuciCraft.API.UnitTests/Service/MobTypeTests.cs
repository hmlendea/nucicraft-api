using System;
using System.Reflection;

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
        public void GivenANullExternalName_WhenConvertingFromString_ThenUnsupportedIsReturned()
            => Assert.That(
                MobType.FromString(null),
                Is.EqualTo(MobType.Unsupported));

        [Test]
        public void GivenAWhitespaceExternalName_WhenConvertingFromString_ThenUnsupportedIsReturned()
            => Assert.That(
                MobType.FromString(" "),
                Is.EqualTo(MobType.Unsupported));

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

        [Test]
        public void GivenAllMobTypes_WhenGettingValues_ThenEveryMobTypeIsReturned()
            => Assert.That(
                MobType.GetValues(),
                Has.Length.EqualTo(10));

        [Test]
        public void GivenNull_WhenComparingTypedMobTypes_ThenFalseIsReturned()
            => Assert.That(
                MobType.Cow.Equals((MobType)null),
                Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingTypedMobTypes_ThenTrueIsReturned()
            => Assert.That(MobType.Cow.Equals(MobType.Cow));

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingTypedMobTypes_ThenTrueIsReturned()
        {
            MobType mobType = BuildMobType(
                nameof(MobType.Cow),
                "cow");

            Assert.That(MobType.Cow.Equals(mobType));
        }

        [Test]
        public void GivenMatchingNamesAndDifferentExternalNames_WhenComparingTypedMobTypes_ThenFalseIsReturned()
        {
            MobType mobType = BuildMobType(
                nameof(MobType.Cow),
                "pig");

            Assert.That(MobType.Cow.Equals(mobType), Is.False);
        }

        [Test]
        public void GivenDifferentMobTypes_WhenComparingTypedMobTypes_ThenFalseIsReturned()
            => Assert.That(
                MobType.Cow.Equals(MobType.Pig),
                Is.False);

        [Test]
        public void GivenNull_WhenComparingObjectMobTypes_ThenFalseIsReturned()
            => Assert.That(
                MobType.Cow.Equals((object)null),
                Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingObjectMobTypes_ThenTrueIsReturned()
            => Assert.That(MobType.Cow.Equals((object)MobType.Cow));

        [Test]
        public void GivenAnotherType_WhenComparingObjectMobTypes_ThenFalseIsReturned()
            => Assert.That(
                MobType.Cow.Equals("cow"),
                Is.False);

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingObjectMobTypes_ThenTrueIsReturned()
        {
            object mobType = BuildMobType(
                nameof(MobType.Cow),
                "cow");

            Assert.That(MobType.Cow.Equals(mobType));
        }

        [Test]
        public void GivenAnEquivalentMobType_WhenGettingHashCodes_ThenTheHashCodesAreEqual()
        {
            MobType mobType = BuildMobType(
                nameof(MobType.Cow),
                "cow");

            Assert.That(
                MobType.Cow.GetHashCode(),
                Is.EqualTo(mobType.GetHashCode()));
        }

        [Test]
        public void GivenAMobType_WhenCallingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                MobType.Cow.ToString(),
                Is.EqualTo("cow"));

        [Test]
        public void GivenTwoNullMobTypes_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            MobType current = null;
            MobType other = null;

            Assert.That(current == other);
        }

        [Test]
        public void GivenNullAndNonNullMobTypes_WhenUsingTheEqualityOperator_ThenFalseIsReturned()
        {
            MobType current = null;

            Assert.That(current == MobType.Cow, Is.False);
        }

        [Test]
        public void GivenEquivalentMobTypes_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            MobType mobType = BuildMobType(
                nameof(MobType.Cow),
                "cow");

            Assert.That(MobType.Cow == mobType);
        }

        [Test]
        public void GivenDifferentMobTypes_WhenUsingTheInequalityOperator_ThenTrueIsReturned()
            => Assert.That(MobType.Cow != MobType.Pig);

        private static MobType BuildMobType(
            string name,
            string externalName)
            => Activator.CreateInstance(
                typeof(MobType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [name, externalName],
                null) as MobType;
    }
}