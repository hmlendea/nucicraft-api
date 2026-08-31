using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Requests
{
    public class AddWorldRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedStringDataObject Name { get; set; }

        [HmacOrder(3)]
        public bool HasWebMap { get; set; }

        [HmacOrder(4)]
        public CoordinatesDataObject SpawnPoint { get; set; }

        [HmacOrder(5)]
        public string Type { get; set; }
    }
}
