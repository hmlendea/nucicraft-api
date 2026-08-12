using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class AddCountryRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(3)]
        public LocalisedStringDataObject LeaderTitle { get; set; }

        [HmacOrder(4)]
        public string Leader { get; set; }
    }
}
