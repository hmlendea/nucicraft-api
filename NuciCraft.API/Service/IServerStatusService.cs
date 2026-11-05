using System.Threading;
using System.Threading.Tasks;

namespace NuciCraft.API.Service
{
    public interface IServerStatusService
    {
        Task<int> GetOnlinePlayersCountAsync();

        Task<int> GetOnlinePlayersCountAsync(CancellationToken cancellationToken);
    }
}