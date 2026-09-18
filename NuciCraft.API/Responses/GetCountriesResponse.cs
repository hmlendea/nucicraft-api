using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetCountriesResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<Country> Countries { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (Countries is null)
                {
                    return 0;
                }

                return Countries.Count();
            }
        }
    }
}