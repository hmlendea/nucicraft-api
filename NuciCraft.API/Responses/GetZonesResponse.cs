using System.Collections.Generic;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;
using System.Linq;

namespace NuciCraft.API.Responses
{
    public class GetZonesResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public IEnumerable<Zone> Zones { get; set; }

        [HmacIgnore]
        public int Count => Zones?.Count() ?? 0;
    }
}
