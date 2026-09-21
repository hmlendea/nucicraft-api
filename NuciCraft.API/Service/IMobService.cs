using System.Collections.Generic;

using NuciCraft.API.Requests;

namespace NuciCraft.API.Service
{
    public interface IMobService
    {
        IEnumerable<string> GetRandomMobName(GetMobNameRequest request);
    }
}