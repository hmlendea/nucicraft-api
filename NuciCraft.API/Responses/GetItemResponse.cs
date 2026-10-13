using System.Collections.Generic;
using System.Text.Json.Serialization;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetItemResponse(Item item) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        [JsonPropertyName("id")]
        public string Identifier { get; set; } = item.Identifier;

        [HmacOrder(2)]
        public string MinecraftId { get; set; } = item.MinecraftId;

        [HmacOrder(3)]
        public string BukkitId { get; set; } = item.BukkitId;

        [HmacOrder(4)]
        public List<string> SignIds { get; set; } = item.SignIds;
    }
}