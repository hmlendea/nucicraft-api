using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class PatchWorldRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }
    }
}
