using System.Collections.Generic;

using NUnit.Framework;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetWorldsResponseTests
    {
        [Test]
        public void GivenNullWorlds_WhenGettingTheCount_ThenZeroIsReturned()
        {
            GetWorldsResponse response = new()
            {
                Worlds = null
            };

            Assert.That(response.Count, Is.Zero);
        }

        [Test]
        public void GivenTwoWorlds_WhenGettingTheCount_ThenTwoIsReturned()
        {
            IEnumerable<World> worlds =
            [
                new(),
                new(),
            ];
            GetWorldsResponse response = new()
            {
                Worlds = worlds
            };

            Assert.That(response.Count, Is.EqualTo(2));
        }
    }
}
