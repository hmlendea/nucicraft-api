using System;

using NUnit.Framework;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class PlayerTests
    {
        private static DateTimeOffset CreatedDateTime
            => new(2012, 9, 5, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset LatestDateTime
            => new(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);

        [Test]
        public void GivenOnlyACreatedTimestamp_WhenAccessingLastSeenDT_ThenCreatedDTIsReturned()
        {
            Player player = BuildPlayer();

            Assert.That(player.LastSeenDT, Is.EqualTo(CreatedDateTime));
        }

        [Test]
        public void GivenALaterLoginTimestamp_WhenAccessingLastSeenDT_ThenLastLoginDTIsReturned()
        {
            Player player = BuildPlayer();
            player.LastLoginDT = LatestDateTime;

            Assert.That(player.LastSeenDT, Is.EqualTo(LatestDateTime));
        }

        [Test]
        public void GivenALaterLogoutTimestamp_WhenAccessingLastSeenDT_ThenLastLogoutDTIsReturned()
        {
            Player player = BuildPlayer();
            player.LastLogoutDT = LatestDateTime;

            Assert.That(player.LastSeenDT, Is.EqualTo(LatestDateTime));
        }

        [Test]
        public void GivenALaterSleptTimestamp_WhenAccessingLastSeenDT_ThenLastSleptDTIsReturned()
        {
            Player player = BuildPlayer();
            player.LastSleptDT = LatestDateTime;

            Assert.That(player.LastSeenDT, Is.EqualTo(LatestDateTime));
        }

        [Test]
        public void GivenALaterDeathTimestamp_WhenAccessingLastSeenDT_ThenLastDeathDTIsReturned()
        {
            Player player = BuildPlayer();
            player.LastDeathDT = LatestDateTime;

            Assert.That(player.LastSeenDT, Is.EqualTo(LatestDateTime));
        }

        [Test]
        public void GivenALaterBackTimestamp_WhenAccessingLastSeenDT_ThenBackDTIsReturned()
        {
            Player player = BuildPlayer();
            player.BackDT = LatestDateTime;

            Assert.That(player.LastSeenDT, Is.EqualTo(LatestDateTime));
        }

        private static Player BuildPlayer() => new()
        {
            CreatedDT = CreatedDateTime
        };
    }
}