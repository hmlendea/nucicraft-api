using System;
using System.Collections.Generic;
using System.Globalization;

using NuciLog.Core;

using NuciCraft.API.Logging;
using NuciCraft.API.Requests;
using NuciCraft.API.Service.Mapping;

namespace NuciCraft.API.Service
{
    public class GameEventService(
        IPlayerService playerService,
        ILogger logger) : IGameEventService
    {
        private static string TimestampFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

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

            playerService.Update(new UpdatePlayerRequest
            {
                PlayerUsername = request.Player,
                LastDeathDT = DateTimeOffset.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture),
                LastDeathLocation = request.DeathLocation.ToDataObject()
            });
        }
    }
}
