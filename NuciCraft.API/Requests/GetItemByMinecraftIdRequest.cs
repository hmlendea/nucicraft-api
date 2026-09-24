using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetItemByMinecraftIdRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("minecraftId")]
        public string MinecraftId { get; set; }
    }
}