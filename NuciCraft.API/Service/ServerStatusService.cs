using System;
using System.IO;
using System.Net.Sockets;

using MineStatLib;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.Service
{
    public sealed class ServerStatusService(ServerSettings settings) : IServerStatusService
    {
        private static int QueryTimeoutSeconds => 5;

        public int GetOnlinePlayersCount()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.Hostname);
            ArgumentOutOfRangeException.ThrowIfLessThan(settings.JavaEditionPort, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(settings.JavaEditionPort, ushort.MaxValue);

            MineStat status;

            try
            {
                status = new(
                    settings.Hostname,
                    (ushort)settings.JavaEditionPort,
                    QueryTimeoutSeconds,
                    SlpProtocol.Legacy);
            }
            catch (IOException)
            {
                return 0;
            }
            catch (SocketException)
            {
                return 0;
            }

            if (!status.ServerUp)
            {
                return 0;
            }

            if (status.CurrentPlayersInt < 0)
            {
                throw new InvalidOperationException(
                    "The Java server did not return a valid online player count.");
            }

            return status.CurrentPlayersInt;
        }
    }
}