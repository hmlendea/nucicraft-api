using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class MobTypeTests
    {
        [Test]
        public void GivenAKnownExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("wandering_trader");

            Assert.That(mobType, Is.EqualTo(MobType.WanderingTrader));
        }

        [Test]
        public void GivenTheEnderDragonExternalName_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("ender_dragon");

            Assert.That(mobType, Is.EqualTo(MobType.EnderDragon));
        }

        [Test]
        public void GivenAKnownExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("WANDERING_TRADER");

            Assert.That(mobType, Is.EqualTo(MobType.WanderingTrader));
        }

        [Test]
        public void GivenTheEnderDragonExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("ENDER_DRAGON");

            Assert.That(mobType, Is.EqualTo(MobType.EnderDragon));
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
        public void GivenTheEnderDragonMobType_WhenConvertingToString_ThenTheExternalNameIsReturned()
            => Assert.That(
                (string)MobType.EnderDragon,
                Is.EqualTo("ender_dragon"));
    }
}