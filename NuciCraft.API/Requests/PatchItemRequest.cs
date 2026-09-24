using System.Collections.Generic;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class PatchItemRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        [JsonPropertyName("minecraftId")]
        public string MinecraftId { get; set; }

        [HmacOrder(3)]
        [JsonPropertyName("bukkitId")]
        public string BukkitId { get; set; }

        [HmacOrder(4)]
        [JsonPropertyName("signIds")]
        public List<string> SignIds { get; set; }
    }
}