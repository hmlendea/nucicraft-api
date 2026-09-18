using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public sealed class GetZoneIdentifiersResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<string> ZoneIdentifiers { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (ZoneIdentifiers is null)
                {
                    return 0;
                }

                return ZoneIdentifiers.Count();
            }
        }
    }
}