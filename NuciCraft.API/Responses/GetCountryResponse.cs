using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetCountryResponse(Country country) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Identifier { get; set; } = country.Identifier;

        [HmacOrder(2)]
        public LocalisedString Name { get; set; } = country.Name;

        [HmacOrder(3)]
        public LocalisedString LeaderTitle { get; set; } = country.LeaderTitle;

        [HmacOrder(4)]
        public string Leader { get; set; } = country.Leader;
    }
}