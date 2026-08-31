using System;
using System.Reflection;

using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class WorldTypeTests
    {
        [TestCase("overworld", nameof(WorldType.Overworld))]
        [TestCase("nether", nameof(WorldType.Nether))]
        [TestCase("end", nameof(WorldType.End))]
        [TestCase("OVERWORLD", nameof(WorldType.Overworld))]
        [TestCase("NETHER", nameof(WorldType.Nether))]
        [TestCase("END", nameof(WorldType.End))]
        public void GivenAKnownExternalName_WhenConvertingFromString_ThenTheMatchingWorldTypeIsReturned(
            string externalName,
            string expectedName)
            => Assert.That(
                WorldType.FromString(externalName).Name,
                Is.EqualTo(expectedName));

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("unknown")]
        public void GivenNoKnownExternalName_WhenConvertingFromString_ThenOverworldIsReturned(string externalName)
            => Assert.That(
                WorldType.FromString(externalName),
                Is.EqualTo(WorldType.Overworld));

        [TestCase("overworld")]
        [TestCase("nether")]
        [TestCase("end")]
        public void GivenAWorldType_WhenConvertingToString_ThenTheExternalNameIsReturned(string externalName)
        {
            WorldType worldType = WorldType.FromString(externalName);

            Assert.That((string)worldType, Is.EqualTo(externalName));
            Assert.That(worldType.ToString(), Is.EqualTo(externalName));
        }

        [Test]
        public void GivenAllWorldTypes_WhenGettingValues_ThenEveryWorldTypeIsReturned()
            => Assert.That(WorldType.GetValues(), Has.Length.EqualTo(3));

        [Test]
        public void GivenNull_WhenComparingTypedWorldTypes_ThenFalseIsReturned()
            => Assert.That(WorldType.Overworld.Equals((WorldType)null), Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingTypedWorldTypes_ThenTrueIsReturned()
            => Assert.That(WorldType.Overworld.Equals(WorldType.Overworld));

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingTypedWorldTypes_ThenTrueIsReturned()
        {
            WorldType worldType = BuildWorldType(nameof(WorldType.Overworld), "overworld");

            Assert.That(WorldType.Overworld.Equals(worldType));
        }

        [Test]
        public void GivenDifferentWorldTypes_WhenComparingTypedWorldTypes_ThenFalseIsReturned()
            => Assert.That(WorldType.Overworld.Equals(WorldType.Nether), Is.False);

        [Test]
        public void GivenNull_WhenComparingObjectWorldTypes_ThenFalseIsReturned()
            => Assert.That(WorldType.Overworld.Equals((object)null), Is.False);

        [Test]
        public void GivenTheIdenticalInstance_WhenComparingObjectWorldTypes_ThenTrueIsReturned()
            => Assert.That(WorldType.Overworld.Equals((object)WorldType.Overworld));

        [Test]
        public void GivenAnotherType_WhenComparingObjectWorldTypes_ThenFalseIsReturned()
            => Assert.That(WorldType.Overworld.Equals("overworld"), Is.False);

        [Test]
        public void GivenDistinctEquivalentInstances_WhenComparingObjectWorldTypes_ThenTrueIsReturned()
        {
            object worldType = BuildWorldType(nameof(WorldType.Overworld), "overworld");

            Assert.That(WorldType.Overworld.Equals(worldType));
        }

        [Test]
        public void GivenAnEquivalentWorldType_WhenGettingHashCodes_ThenTheHashCodesAreEqual()
        {
            WorldType worldType = BuildWorldType(nameof(WorldType.Overworld), "overworld");

            Assert.That(
                WorldType.Overworld.GetHashCode(),
                Is.EqualTo(worldType.GetHashCode()));
        }

        [Test]
        public void GivenTwoNullWorldTypes_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            WorldType current = null;
            WorldType other = null;

            Assert.That(current == other);
        }

        [Test]
        public void GivenNullAndANonNullWorldType_WhenUsingTheEqualityOperator_ThenFalseIsReturned()
        {
            WorldType current = null;

            Assert.That(current == WorldType.Overworld, Is.False);
        }

        [Test]
        public void GivenEquivalentWorldTypes_WhenUsingTheEqualityOperator_ThenTrueIsReturned()
        {
            WorldType worldType = BuildWorldType(nameof(WorldType.Overworld), "overworld");

            Assert.That(WorldType.Overworld == worldType);
        }

        [Test]
        public void GivenDifferentWorldTypes_WhenUsingTheInequalityOperator_ThenTrueIsReturned()
            => Assert.That(WorldType.Overworld != WorldType.Nether);

        private static WorldType BuildWorldType(string name, string externalName)
            => Activator.CreateInstance(
                typeof(WorldType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [name, externalName],
                null) as WorldType;
    }
}