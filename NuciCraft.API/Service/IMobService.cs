using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NuciCraft.API.Requests;

namespace NuciCraft.API.Service
{
    public interface IMobService
    {
        Task<IEnumerable<string>> GetRandomMobNameAsync(GetMobNameRequest request);
        Task<IEnumerable<string>> GetRandomMobNameAsync(GetMobNameRequest request, CancellationToken cancellationToken);
    }
}