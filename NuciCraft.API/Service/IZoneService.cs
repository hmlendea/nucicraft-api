using System.Collections.Generic;

using NuciCraft.API.Requests;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IZoneService
    {
        void Add(AddZoneRequest request);

        Zone GetZone(string zoneIdentifier);

        IEnumerable<Zone> GetAllZones();

        void Update(PatchZoneRequest request);
    }
}
