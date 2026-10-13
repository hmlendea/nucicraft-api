using System.Collections.Generic;
using System.Linq;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetItemsResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public IEnumerable<Item> Items { get; set; }

        [HmacIgnore]
        public int Count
        {
            get
            {
                if (Items is null)
                {
                    return 0;
                }

                return Items.Count();
            }
        }
    }
}