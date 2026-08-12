using System.Collections.Generic;

using NuciCraft.API.Requests;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service
{
    public interface ICountryService
    {
        void Add(AddCountryRequest request);

        Country Get(string countryIdentifier);

        IEnumerable<Country> GetAll();

        void Update(PatchCountryRequest request);
    }
}
