using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using NuciCraft.API.Service.Generators;

namespace NuciCraft.API.UnitTests.Service.Generators
{
    public class SignIdGeneratorTests
    {
        private SignIdGenerator signIdGenerator;

        [SetUp]
        public void Setup()
        {
            signIdGenerator = new();
        }

        [TestCase("acacia_door", "AcaciaDoor")]
        [TestCase("bamboo_slab", "BambooSlab")]
        [TestCase("diamond_leggings", "DiamondLeggings")]
        [TestCase("minecraft:iron_pickaxe", "IronPickaxe")]
        [TestCase("polished_deepslate_bricks", "PolishedDeepslateBricks")]
        public void GenerateDefaultSignIds_ReturnsExpectedSignId(
            string minecraftId,
            string expectedSignId)
        {
            List<string> result = signIdGenerator.GenerateDefaultSignIds(minecraftId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First(), Is.EqualTo(expectedSignId));
        }
    }
}
