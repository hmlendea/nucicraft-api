using System.Collections.Generic;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NuciAPI.Responses;

using NUnit.Framework;

using NuciCraft.API.Controllers;
using NuciCraft.API.Requests;
using NuciCraft.API.Responses;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Controllers
{
    [TestFixture]
    public sealed class ItemsControllerTests
    {
        private Mock<IItemService> serviceMock;
        private ItemsController controller;

        [SetUp]
        public void SetUp()
        {
            serviceMock = new Mock<IItemService>();
            controller = new ItemsController(
                serviceMock.Object,
                ControllerTestContext.BuildSecuritySettings());
            ControllerTestContext.Initialise(controller);
        }

        [Test]
        public void GivenAnAddRequest_WhenAddingAnItem_ThenTheServiceReceivesTheRequest()
        {
            AddItemRequest request = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND"
            };

            OkObjectResult result = controller.Add(request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.Add(request), Times.Once);
        }

        [Test]
        public void GivenAnItemIdentifier_WhenGettingAnItem_ThenTheItemIsReturned()
        {
            Item item = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND"
            };
            serviceMock.Setup(service => service.Get("test_item")).Returns(item);

            OkObjectResult result = controller.Get("test_item") as OkObjectResult;
            NuciApiContentResponse<GetItemResponse> response =
                result.Value as NuciApiContentResponse<GetItemResponse>;
            GetItemResponse content = response.Content;

            Assert.That(content.Identifier, Is.EqualTo(item.Identifier));
            Assert.That(content.MinecraftId, Is.EqualTo(item.MinecraftId));
            Assert.That(content.BukkitId, Is.EqualTo(item.BukkitId));
        }

        [Test]
        public void GivenAMinecraftId_WhenGettingAnItemByMinecraftId_ThenTheServiceIsCalled()
        {
            serviceMock.Setup(service => service.GetByMinecraftId("minecraft:diamond"))
                .Returns(new Item() { Identifier = "test_item" });

            OkObjectResult result = controller.GetByMinecraftId("minecraft:diamond") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.GetByMinecraftId("minecraft:diamond"), Times.Once);
        }

        [Test]
        public void GivenABukkitId_WhenGettingAnItemByBukkitId_ThenTheServiceIsCalled()
        {
            serviceMock.Setup(service => service.GetByBukkitId("DIAMOND"))
                .Returns(new Item() { Identifier = "test_item" });

            OkObjectResult result = controller.GetByBukkitId("DIAMOND") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.GetByBukkitId("DIAMOND"), Times.Once);
        }

        [Test]
        public void GivenANuciCraftId_WhenGettingAnItemByNuciCraftId_ThenTheServiceIsCalled()
        {
            serviceMock.Setup(service => service.GetByNuciCraftId("nucicraft:diamond"))
                .Returns(new Item() { Identifier = "test_item" });

            OkObjectResult result = controller.GetByNuciCraftId("nucicraft:diamond") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            serviceMock.Verify(service => service.GetByNuciCraftId("nucicraft:diamond"), Times.Once);
        }

        [Test]
        public void GivenItems_WhenGettingAllItems_ThenTheItemsAreReturned()
        {
            IEnumerable<Item> items =
            [
                new() { Identifier = "test_item" }
            ];
            serviceMock.Setup(service => service.GetAll()).Returns(items);

            OkObjectResult result = controller.GetAll() as OkObjectResult;
            NuciApiContentResponse<GetItemsResponse> response =
                result.Value as NuciApiContentResponse<GetItemsResponse>;
            GetItemsResponse content = response.Content;

            Assert.That(content.Items, Is.SameAs(items));
        }

        [Test]
        public void GivenAnItemIdentifier_WhenPatchingAnItem_ThenTheIdentifierIsAppliedToTheRequest()
        {
            PatchItemRequest request = new();

            OkObjectResult result = controller.PatchByIdentifier("test_item", request) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(request.Identifier, Is.EqualTo("test_item"));
            serviceMock.Verify(service => service.Update(request), Times.Once);
        }

        [Test]
        public void GivenTheGetByMinecraftIdAction_WhenInspectingItsRoute_ThenItUsesTheMinecraftIdSegment()
        {
            MethodInfo actionMethod = typeof(ItemsController)
                .GetMethod(nameof(ItemsController.GetByMinecraftId));
            RouteAttribute routeAttribute = actionMethod.GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute.Template, Is.EqualTo("by-minecraft-id/{minecraftId}"));
        }

        [Test]
        public void GivenTheGetByBukkitIdAction_WhenInspectingItsRoute_ThenItUsesTheBukkitIdSegment()
        {
            MethodInfo actionMethod = typeof(ItemsController)
                .GetMethod(nameof(ItemsController.GetByBukkitId));
            RouteAttribute routeAttribute = actionMethod.GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute.Template, Is.EqualTo("by-bukkit-id/{bukkitId}"));
        }

        [Test]
        public void GivenTheGetByNuciCraftIdAction_WhenInspectingItsRoute_ThenItUsesTheNuciCraftIdSegment()
        {
            MethodInfo actionMethod = typeof(ItemsController)
                .GetMethod(nameof(ItemsController.GetByNuciCraftId));
            RouteAttribute routeAttribute = actionMethod.GetCustomAttribute<RouteAttribute>();

            Assert.That(routeAttribute.Template, Is.EqualTo("by-nucicraft-id/{nuciCraftId}"));
        }
    }
}
