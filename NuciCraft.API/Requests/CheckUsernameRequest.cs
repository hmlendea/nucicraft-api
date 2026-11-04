using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using NuciAPI.Requests;

namespace NuciCraft.API.Requests
{
    public sealed class CheckUsernameRequest : NuciApiRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }
}