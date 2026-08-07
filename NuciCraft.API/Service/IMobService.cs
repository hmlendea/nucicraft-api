using NuciCraft.API.Requests;

namespace NuciCraft.API.Service
{
    public interface IMobService
    {
        string GetRandomMobName(GetMobNameRequest request);
    }
}