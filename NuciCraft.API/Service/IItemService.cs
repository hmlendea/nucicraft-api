using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IItemService
    {
        void Add(AddItemRequest request);

        Item Get(string itemIdentifier);

        Item GetByMinecraftId(string minecraftId);

        IEnumerable<Item> GetAll();

        void Update(PatchItemRequest request);
    }
}