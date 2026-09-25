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

        [TestCase("polished_cinnabar_stairs", "PoliCinbarStair")]
        [TestCase("acacia_door", "AcaciaDoor")]
        [TestCase("bamboo_slab", "BambooSlab")]
        [TestCase("diamond_leggings", "DiamondLeggings")]
        [TestCase("minecraft:copper_axe", "CopperHatchet")]
        [TestCase("minecraft:iron_pickaxe", "IronPickaxe")]
        [TestCase("minecraft:netherite_upgrade_smithing_template", "NthrUpgSmithTemp")]
        [TestCase("minecraft:pale_oak_trapdoor", "PaleOakTrapdoor")]
        [TestCase("pearlescent_froglight", "PearlFroglight")]
        [TestCase("polished_deepslate_bricks", "PolishDeepBricks")]
        [TestCase("sulfur", "Sulphur")]
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
