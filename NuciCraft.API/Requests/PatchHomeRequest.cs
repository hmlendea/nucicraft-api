using System.Text.Json.Serialization;

using NuciAPI.Requests;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Requests
{
    public sealed class PatchHomeRequest : NuciApiRequest
    {
        [JsonIgnore]
        [HmacOrder(1)]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public LocalisedString Name { get; set; }

        [HmacOrder(3)]
        public string Player { get; set; }

        [HmacOrder(4)]
        public Coordinates Location { get; set; }
    }
}