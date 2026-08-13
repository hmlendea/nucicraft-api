using System.Collections.Generic;

using NUnit.Framework;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetZonesResponseTests
    {
        [Test]
        public void GivenNullZones_WhenGettingTheCount_ThenZeroIsReturned()
        {
            GetZonesResponse response = new()
            {
                Zones = null
            };

            Assert.That(response.Count, Is.Zero);
        }

        [Test]
        public void GivenTwoZones_WhenGettingTheCount_ThenTwoIsReturned()
        {
            IEnumerable<Zone> zones =
            [
                new(),
                new(),
            ];
            GetZonesResponse response = new()
            {
                Zones = zones
            };

            Assert.That(response.Count, Is.EqualTo(2));
        }
    }
}