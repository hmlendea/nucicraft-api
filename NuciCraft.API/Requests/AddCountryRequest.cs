using System.ComponentModel.DataAnnotations;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;
using System.Text.Json.Serialization;

namespace NuciCraft.API.Requests
{
    public class AddCountryRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        [Required]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(3)]
        public LocalisedStringDataObject LeaderTitle { get; set; }

        [HmacOrder(4)]
        public string Leader { get; set; }
    }
}
