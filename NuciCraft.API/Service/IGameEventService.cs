using NuciCraft.API.Requests;

namespace NuciCraft.API.Service
{
    public interface IGameEventService
    {
        void HandlePlayerDeath(NotifyPlayerDeathRequest request);
    }
}
