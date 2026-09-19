using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IHomeService
    {
        Home Add(AddHomeRequest request);

        Home Get(string homeIdentifier);

        Home Get(string playerIdentifier, string name);

        IEnumerable<Home> GetAll();

        IEnumerable<Home> GetAllByPlayer(string playerIdentifier);

        Home Update(PatchHomeRequest request);
    }
}