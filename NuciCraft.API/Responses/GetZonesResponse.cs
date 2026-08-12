using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetZonesResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public IEnumerable<Zone> Zones { get; set; }

        [HmacIgnore]
        public int Count => Zones?.Count() ?? 0;
    }
}
