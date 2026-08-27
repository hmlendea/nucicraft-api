using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IWorldService
    {
        void Add(AddWorldRequest request);

        World GetWorld(string worldIdentifier);

        IEnumerable<World> GetAllWorlds();

        void Update(PatchWorldRequest request);
    }
}
