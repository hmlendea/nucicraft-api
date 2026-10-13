using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class AddItemRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [Required]
        [HmacOrder(2)]
        [JsonPropertyName("minecraftId")]
        public string MinecraftId { get; set; }

        [HmacOrder(3)]
        [JsonPropertyName("nucicraftId")]
        public string NuciCraftId { get; set; }

        [Required]
        [HmacOrder(4)]
        [JsonPropertyName("bukkitId")]
        public string BukkitId { get; set; }

        [HmacOrder(5)]
        [JsonPropertyName("signIds")]
        public List<string> SignIds { get; set; }
    }
}