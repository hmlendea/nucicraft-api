using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetHomesResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<Home> Homes { get; set; } = [];

        [HmacIgnore]
        public int Count => Homes.Count();
    }
}