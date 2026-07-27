using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IZoneService
    {
        Zone GetZone(string zoneIdentifier);
    }
}
