using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IPlayerService
    {
        void Register(RegisterPlayerRequest request);

        Player Get(GetPlayerRequest request);

        void UpdateLastDeathLocation(string username, Coordinates location);
    }
}
