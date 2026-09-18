using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetZoneTypesResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<ZoneType> ZoneTypes { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (ZoneTypes is null)
                {
                    return 0;
                }

                return ZoneTypes.Count();
            }
        }
    }
}