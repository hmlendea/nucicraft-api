using NUnit.Framework;

using NuciCraft.API.Service;

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
        public void GivenAKnownExternalNameWithDifferentCasing_WhenConvertingFromString_ThenTheMatchingMobTypeIsReturned()
        {
            MobType mobType = MobType.FromString("WANDERING_TRADER");

            Assert.That(mobType, Is.EqualTo(MobType.WanderingTrader));
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
    }
}