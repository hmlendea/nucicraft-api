using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using NuciCraft.API.Service;
using System.Reflection;

namespace NuciCraft.API.UnitTests.Service
{
    public class ItemServiceTests
    {
        [TestCase("acacia_door", "AcaciaDoor")]
        [TestCase("bamboo_slab", "BambooSlab")]
        [TestCase("polished_deepslate_bricks", "PolishedDeepslateBricks")]
        [TestCase("diamond_leggings", "DiamondLeggings")]
        public void GenerateDefaultSignIds_ReturnsExpectedSignId(string minecraftId, string expectedSignId)
        {
            var result = InvokeGenerateDefaultSignIds(minecraftId);

            Assert.NotNull(result);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First(), Is.EqualTo(expectedSignId));
        }

        private static List<string> InvokeGenerateDefaultSignIds(string minecraftId)
        {
            MethodInfo method = typeof(ItemService).GetMethod(
                "GenerateDefaultSignIds",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (List<string>)method.Invoke(null, [minecraftId]);
        }
    }
}
