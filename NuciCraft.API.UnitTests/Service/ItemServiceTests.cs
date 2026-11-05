using System;
using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using NuciDAL.Repositories;

using NuciLog.Core;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Generators;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public sealed class ItemServiceTests
    {
        private Mock<IFileRepository<ItemDataObject>> repositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<ISignIdGenerator> signIdGeneratorMock;
        private ItemService itemService;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IFileRepository<ItemDataObject>>();
            loggerMock = new Mock<ILogger>();
            signIdGeneratorMock = new Mock<ISignIdGenerator>();
            itemService = new ItemService(repositoryMock.Object, loggerMock.Object, signIdGeneratorMock.Object);
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAnItem_ThenTheItemIsPersisted()
        {
            ItemDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ItemDataObject>()))
                .Callback<ItemDataObject>(entity => capturedEntity = entity);

            signIdGeneratorMock
                .Setup(generator => generator.GenerateDefaultSignIds(It.IsAny<string>()))
                .Returns(["TestSignId"]);

            AddItemRequest request = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                NuciCraftId = "nucicraft:diamond",
                BukkitId = "DIAMOND",
                SignIds = null
            };

            itemService.Add(request);

            Assert.That(capturedEntity, Is.Not.Null);
            Assert.That(capturedEntity.Id, Is.EqualTo("test_item"));
            Assert.That(capturedEntity.MinecraftId, Is.EqualTo("minecraft:diamond"));
            Assert.That(capturedEntity.NuciCraftId, Is.EqualTo("nucicraft:diamond"));
            Assert.That(capturedEntity.BukkitId, Is.EqualTo("DIAMOND"));
            Assert.That(capturedEntity.SignIds, Is.EqualTo(["TestSignId"]));
        }

        [Test]
        public void GivenAValidRequest_WhenAddingAnItem_ThenSaveChangesIsInvoked()
        {
            signIdGeneratorMock
                .Setup(generator => generator.GenerateDefaultSignIds(It.IsAny<string>()))
                .Returns(["TestSignId"]);

            AddItemRequest request = new()
            {
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND"
            };

            itemService.Add(request);

            repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
        }

        [Test]
        public void GivenARepositoryException_WhenAddingAnItem_ThenTheExceptionIsRethrown()
        {
            signIdGeneratorMock
                .Setup(generator => generator.GenerateDefaultSignIds(It.IsAny<string>()))
                .Returns(["TestSignId"]);

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ItemDataObject>()))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => itemService.Add(new AddItemRequest
                {
                    MinecraftId = "minecraft:diamond",
                    BukkitId = "DIAMOND"
                }),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenANullRequest_WhenAddingAnItem_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.That(
                () => itemService.Add(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenMissingMinecraftId_WhenAddingAnItem_ThenAnArgumentExceptionIsThrown()
        {
            AddItemRequest request = new()
            {
                MinecraftId = "",
                BukkitId = "DIAMOND"
            };

            Assert.That(
                () => itemService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenMissingBukkitId_WhenAddingAnItem_ThenAnArgumentExceptionIsThrown()
        {
            AddItemRequest request = new()
            {
                MinecraftId = "minecraft:diamond",
                BukkitId = ""
            };

            Assert.That(
                () => itemService.Add(request),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenAnExistingItem_WhenGettingAnItem_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.Get("test_item"))
                .Returns(BuildItemDataObject());

            Item item = itemService.Get("test_item");

            Assert.That(item.Identifier, Is.EqualTo("test_item"));
            Assert.That(item.MinecraftId, Is.EqualTo("minecraft:diamond"));
            Assert.That(item.NuciCraftId, Is.EqualTo("nucicraft:diamond"));
            Assert.That(item.BukkitId, Is.EqualTo("DIAMOND"));
            Assert.That(item.SignIds, Is.EqualTo(["TestSignId"]));
        }

        [Test]
        public void GivenANonExistentItem_WhenGettingAnItem_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("nonexistent"))
                .Returns((ItemDataObject)null);

            Assert.That(
                () => itemService.Get("nonexistent"),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAnItem_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("test_item"))
                .Throws<InvalidOperationException>();

            Assert.That(
                () => itemService.Get("test_item"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAnExistingItemByMinecraftId_WhenGettingByMinecraftId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByMinecraftId("minecraft:diamond");

            Assert.That(item.Identifier, Is.EqualTo("test_item"));
            Assert.That(item.MinecraftId, Is.EqualTo("minecraft:diamond"));
        }

        [Test]
        public void GivenCaseInsensitiveMinecraftId_WhenGettingByMinecraftId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByMinecraftId("MINECRAFT:DIAMOND");

            Assert.That(item.MinecraftId, Is.EqualTo("minecraft:diamond"));
        }

        [Test]
        public void GivenANonExistentMinecraftId_WhenGettingByMinecraftId_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => itemService.GetByMinecraftId("minecraft:nonexistent"),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenAnExistingItemByBukkitId_WhenGettingByBukkitId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByBukkitId("DIAMOND");

            Assert.That(item.Identifier, Is.EqualTo("test_item"));
            Assert.That(item.BukkitId, Is.EqualTo("DIAMOND"));
        }

        [Test]
        public void GivenCaseInsensitiveBukkitId_WhenGettingByBukkitId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByBukkitId("diamond");

            Assert.That(item.BukkitId, Is.EqualTo("DIAMOND"));
        }

        [Test]
        public void GivenANonExistentBukkitId_WhenGettingByBukkitId_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => itemService.GetByBukkitId("NONEXISTENT"),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenAnExistingItemByNuciCraftId_WhenGettingByNuciCraftId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByNuciCraftId("nucicraft:diamond");

            Assert.That(item.Identifier, Is.EqualTo("test_item"));
            Assert.That(item.NuciCraftId, Is.EqualTo("nucicraft:diamond"));
        }

        [Test]
        public void GivenCaseInsensitiveNuciCraftId_WhenGettingByNuciCraftId_ThenTheItemIsReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([BuildItemDataObject()]);

            Item item = itemService.GetByNuciCraftId("NUCICRAFT:DIAMOND");

            Assert.That(item.NuciCraftId, Is.EqualTo("nucicraft:diamond"));
        }

        [Test]
        public void GivenANonExistentNuciCraftId_WhenGettingByNuciCraftId_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([]);

            Assert.That(
                () => itemService.GetByNuciCraftId("nucicraft:nonexistent"),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenItemsInTheRepository_WhenGettingAllItems_ThenAllItemsAreReturned()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Returns([
                    BuildItemDataObject(),
                    new ItemDataObject
                    {
                        Id = "test_item_2",
                        MinecraftId = "minecraft:gold_ingot",
                        NuciCraftId = "nucicraft:gold_ingot",
                        BukkitId = "GOLD_INGOT",
                        SignIds = ["GoldSign"]
                    }
                ]);

            List<Item> items = [.. itemService.GetAll()];

            Assert.That(items, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenARepositoryException_WhenGettingAllItems_ThenTheExceptionIsRethrown()
        {
            repositoryMock
                .Setup(repository => repository.GetAll())
                .Throws<InvalidOperationException>();

            Assert.That(
                itemService.GetAll,
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenARequestWithPartialFields_WhenUpdatingAnItem_ThenOnlyProvidedFieldsAreUpdated()
        {
            ItemDataObject original = BuildItemDataObject();
            ItemDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("test_item"))
                .Returns(original);

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ItemDataObject>()))
                .Callback<ItemDataObject>(entity => capturedEntity = entity);

            PatchItemRequest request = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:netherite_ingot"
            };

            itemService.Update(request);

            Assert.That(capturedEntity.MinecraftId, Is.EqualTo("minecraft:netherite_ingot"));
            Assert.That(capturedEntity.BukkitId, Is.EqualTo(original.BukkitId));
            Assert.That(capturedEntity.SignIds, Is.EqualTo(original.SignIds));
        }

        [Test]
        public void GivenARequestWithAllFields_WhenUpdatingAnItem_ThenAllFieldsAreUpdated()
        {
            ItemDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Get("test_item"))
                .Returns(BuildItemDataObject());

            repositoryMock
                .Setup(repository => repository.Update(It.IsAny<ItemDataObject>()))
                .Callback<ItemDataObject>(entity => capturedEntity = entity);

            PatchItemRequest request = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:netherite_ingot",
                BukkitId = "NETHERITE_INGOT",
                SignIds = ["NetheriteSign"]
            };

            itemService.Update(request);

            Assert.That(capturedEntity.MinecraftId, Is.EqualTo("minecraft:netherite_ingot"));
            Assert.That(capturedEntity.BukkitId, Is.EqualTo("NETHERITE_INGOT"));
            Assert.That(capturedEntity.SignIds, Is.EqualTo(["NetheriteSign"]));
        }

        [Test]
        public void GivenNoIdentifier_WhenUpdatingAnItem_ThenAnArgumentExceptionIsThrown()
        {
            Assert.That(
                () => itemService.Update(new PatchItemRequest
                {
                    MinecraftId = "minecraft:diamond"
                }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenNonExistentItem_WhenUpdatingAnItem_ThenAKeyNotFoundExceptionIsThrown()
        {
            repositoryMock
                .Setup(repository => repository.Get("nonexistent"))
                .Returns((ItemDataObject)null);

            Assert.That(
                () => itemService.Update(new PatchItemRequest
                {
                    Identifier = "nonexistent",
                    MinecraftId = "minecraft:diamond"
                }),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenProvidedSignIds_WhenAddingAnItem_ThenProvidedSignIdsAreUsed()
        {
            ItemDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ItemDataObject>()))
                .Callback<ItemDataObject>(entity => capturedEntity = entity);

            AddItemRequest request = new()
            {
                Identifier = "test_item",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND",
                SignIds = ["CustomSign1", "CustomSign2"]
            };

            itemService.Add(request);

            Assert.That(capturedEntity.SignIds, Is.EqualTo(["CustomSign1", "CustomSign2"]));
        }

        [Test]
        public void GivenAutoGeneratedIdentifier_WhenAddingAnItemWithoutIdentifier_ThenAGuidIsGenerated()
        {
            ItemDataObject capturedEntity = null;

            repositoryMock
                .Setup(repository => repository.Add(It.IsAny<ItemDataObject>()))
                .Callback<ItemDataObject>(entity => capturedEntity = entity);

            signIdGeneratorMock
                .Setup(generator => generator.GenerateDefaultSignIds(It.IsAny<string>()))
                .Returns(["TestSignId"]);

            AddItemRequest request = new()
            {
                Identifier = "",
                MinecraftId = "minecraft:diamond",
                BukkitId = "DIAMOND"
            };

            itemService.Add(request);

            Assert.That(capturedEntity.Id, Does.Match(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
        }

        private static ItemDataObject BuildItemDataObject() => new()
        {
            Id = "test_item",
            MinecraftId = "minecraft:diamond",
            NuciCraftId = "nucicraft:diamond",
            BukkitId = "DIAMOND",
            SignIds = ["TestSignId"]
        };
    }
}