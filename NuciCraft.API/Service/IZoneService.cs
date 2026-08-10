using System.Collections.Generic;

using NuciCraft.API.Requests;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IZoneService
    {
        Zone GetZone(string zoneIdentifier);

        IEnumerable<Zone> GetAllZones();

        void Update(UpdateZoneRequest request);
    }
}
