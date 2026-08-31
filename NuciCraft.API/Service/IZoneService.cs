using System.Collections.Generic;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IZoneService
    {
        void Add(AddZoneRequest request);

        void Delete(string zoneIdentifier);

        Zone GetZone(string zoneIdentifier);

        IEnumerable<Zone> GetAllZones();

        IEnumerable<string> GetZoneIdentifiersContainingCoordinates(CoordinatesDataObject coordinates);

        void Update(PatchZoneRequest request);
    }
}
