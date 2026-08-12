using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class UpdateCountryRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string CountryIdentifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(3)]
        public LocalisedStringDataObject LeaderTitle { get; set; }

        [HmacOrder(4)]
        public string Leader { get; set; }
    }
}
