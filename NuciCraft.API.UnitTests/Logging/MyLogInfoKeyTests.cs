using NUnit.Framework;

using NuciCraft.API.Logging;

namespace NuciCraft.API.UnitTests.Logging
{
    [TestFixture]
    public sealed class MyLogInfoKeyTests
    {
        [Test]
        public void GivenTheSkinUrlKey_WhenAccessingIt_ThenAKeyIsReturned()
            => Assert.That(MyLogInfoKey.SkinUrl, Is.Not.Null);

        [Test]
        public void GivenTheLastIpAddressKey_WhenAccessingIt_ThenAKeyIsReturned()
            => Assert.That(MyLogInfoKey.LastIpAddress, Is.Not.Null);

        [Test]
        public void GivenTheUpdatedTimestampKey_WhenAccessingIt_ThenAKeyIsReturned()
            => Assert.That(MyLogInfoKey.UpdatedDT, Is.Not.Null);
    }
}