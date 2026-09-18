using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public sealed class GetPlayersResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<GetPlayerResponse> Players { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (Players is null)
                {
                    return 0;
                }

                return Players.Count();
            }
        }
    }
}