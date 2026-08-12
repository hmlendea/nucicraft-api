using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IPlayerService
    {
        void Register(RegisterPlayerRequest request);

        Player Get(GetPlayerRequest request);

        IEnumerable<Player> GetAll();

        void Update(PatchPlayerRequest request);
    }
}
