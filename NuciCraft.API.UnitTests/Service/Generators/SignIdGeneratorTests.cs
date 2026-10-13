using System;
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
        [TestCase("bamboo_slab", "BambusSlab")]
        [TestCase("diamond_leggings", "DiamondLeggings")]
        [TestCase("minecraft:cherry_boat", "SakuraBoat")]
        [TestCase("minecraft:copper_axe", "CopperHatchet")]
        [TestCase("minecraft:iron_pickaxe", "IronPickaxe")]
        [TestCase("minecraft:mangrove_propagule", "MangroveSapling")]
        [TestCase("minecraft:netherite_upgrade_smithing_template", "NethUpgSmithTemp")]
        [TestCase("minecraft:pale_oak_trapdoor", "PaleOakTrapdoor")]
        [TestCase("pearlescent_froglight", "PearlFroglight")]
        [TestCase("polished_cinnabar_stairs", "PoliCinbarStair")]
        [TestCase("polished_deepslate_bricks", "PolishDeepBricks")]
        [TestCase("sulfur", "Sulphur")]
        [TestCase("minecraft:diamond_sword", "DiamondSword")]
        [TestCase("minecraft:golden_apple", "GoldenApple")]
        [TestCase("minecraft:enchanted_golden_apple", "EnchGoldenApple")]
        [TestCase("minecraft:netherite_sword", "NetheriteSword")]
        [TestCase("minecraft:chiseled_bookshelf", "ChiselBookshelf")]
        [TestCase("minecraft:stained_glass", "Glass")]
        [TestCase("minecraft:stained_glass_pane", "GlassPane")]
        [TestCase("minecraft:oak_leaves", "OakFoliage")]
        [TestCase("minecraft:birch_leaves", "BirchFoliage")]
        [TestCase("minecraft:bamboo_block", "BambusBlock")]
        [TestCase("minecraft:cherry_sapling", "SakuraSapling")]
        [TestCase("minecraft:cobblestone_slab", "CobbleSlab")]
        [TestCase("minecraft:concrete_powder", "CementPowder")]
        [TestCase("minecraft:deepslate_bricks", "DeepBricks")]
        [TestCase("minecraft:propagule", "Sapling")]
        [TestCase("minecraft:tuff_bricks", "TuffBricks")]
        [TestCase("minecraft:polished_tuff", "PolishedTuff")]
        [TestCase("minecraft:upgrade_smithing_template", "UpgSmithTemplate")]
        [TestCase("minecraft:netherite_ingot", "NetheriteIngot")]
        [TestCase("minecraft:netherite_block", "NetheriteBlock")]
        [TestCase("minecraft:netherite_pickaxe", "NetheritePickaxe")]
        [TestCase("minecraft:netherite_axe", "NetheriteHatchet")]
        [TestCase("minecraft:netherite_shovel", "NetheriteShovel")]
        [TestCase("minecraft:netherite_hoe", "NetheriteHoe")]
        [TestCase("minecraft:netherite_helmet", "NetheriteHelmet")]
        [TestCase("minecraft:netherite_chestplate", "NetherChestplate")]
        [TestCase("minecraft:netherite_leggings", "NetherLeggings")]
        [TestCase("minecraft:netherite_boots", "NetheriteBoots")]
        public void GenerateDefaultSignIds_ReturnsExpectedSignId(
            string minecraftId,
            string expectedSignId)
        {
            List<string> result = signIdGenerator.GenerateDefaultSignIds(minecraftId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First(), Is.EqualTo(expectedSignId));
        }

        [Test]
        public void GenerateDefaultSignIds_EmptyString_ThrowsIndexOutOfRangeException()
        {
            Assert.That(
                () => signIdGenerator.GenerateDefaultSignIds(""),
                Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void GenerateDefaultSignIds_WhitespaceOnly_ThrowsArgumentException()
        {
            Assert.That(
                () => signIdGenerator.GenerateDefaultSignIds("   "),
                Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void GenerateDefaultSignIds_NamespaceOnly_ThrowsIndexOutOfRangeException()
        {
            Assert.That(
                () => signIdGenerator.GenerateDefaultSignIds("minecraft:"),
                Throws.TypeOf<IndexOutOfRangeException>());
        }

        [TestCase("DIAMOND_SWORD", "DiamondSword")]
        [TestCase("diamond_sword", "DiamondSword")]
        [TestCase("Diamond_Sword", "DiamondSword")]
        [TestCase("DiAmOnD_SwOrD", "DiamondSword")]
        public void GenerateDefaultSignIds_CaseInsensitive_ReturnsSameResult(
            string input,
            string expectedSignId)
        {
            List<string> result = signIdGenerator.GenerateDefaultSignIds(input);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First(), Is.EqualTo(expectedSignId));
        }

        [TestCase("minecraft:diamond_sword", "DiamondSword")]
        [TestCase("minecraft:diamond_sword:extra", "DiamondSword:ext")]
        public void GenerateDefaultSignIds_MultipleColons_UsesFirstColon(
            string input,
            string expectedSignId)
        {
            List<string> result = signIdGenerator.GenerateDefaultSignIds(input);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First(), Is.EqualTo(expectedSignId));
        }

        [Test]
        public void GenerateDefaultSignIds_ReturnsSingleItemList()
        {
            List<string> result = signIdGenerator.GenerateDefaultSignIds("minecraft:diamond_sword");

            Assert.That(result, Has.Exactly(1).Items);
        }
    }
}
