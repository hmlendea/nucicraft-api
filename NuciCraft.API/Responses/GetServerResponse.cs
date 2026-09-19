using NuciAPI.Responses;

using NuciSecurity.HMAC;

namespace NuciCraft.API.Responses
{
    public sealed class GetServerResponse : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Name { get; set; } = string.Empty;

        [HmacOrder(2)]
        public string Hostname { get; set; } = string.Empty;

        [HmacOrder(3)]
        public int JavaEditionPort { get; set; }

        [HmacOrder(4)]
        public int BedrockEditionPort { get; set; }
    }
}