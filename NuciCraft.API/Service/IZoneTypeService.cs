using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface IZoneTypeService
    {
        void Add(AddZoneTypeRequest request);

        ZoneType GetZoneType(string zoneTypeIdentifier);

        IEnumerable<ZoneType> GetAllZoneTypes();

        IEnumerable<ZoneType> GetAllZoneTypes(string category);

        void Update(PatchZoneTypeRequest request);
    }
}
