using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetWorldsResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public IEnumerable<World> Worlds { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (Worlds is null)
                {
                    return 0;
                }

                return Worlds.Count();
            }
        }
    }
}
