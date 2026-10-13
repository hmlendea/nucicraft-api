using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public class GetItemByBukkitIdRequest : NuciApiRequest
    {
        [Required]
        [HmacOrder(1)]
        [JsonPropertyName("bukkitId")]
        public string BukkitId { get; set; }
    }
}
