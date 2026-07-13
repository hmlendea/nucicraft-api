using System.Collections.Generic;
using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciLog.Core;

namespace NuciCraft.API.Service
{
    public class GameEventService(
        IPlayerService playerService,
        ILogger logger) : IGameEventService
    {
        public void HandlePlayerDeath(NotifyPlayerDeathRequest request)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(MyLogInfoKey.Username, request.Player),
                new(MyLogInfoKey.World, request.DeathLocation.World),
                new(MyLogInfoKey.X, request.DeathLocation.X),
                new(MyLogInfoKey.Y, request.DeathLocation.Y),
                new(MyLogInfoKey.Z, request.DeathLocation.Z)
            ];

            logger.Info(MyOperation.PlayerDeath, OperationStatus.Started, logInfos);

            playerService.UpdateLastDeathLocation(request.Player, request.DeathLocation);
        }
    }
}
