using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public sealed class GetWorldResponse(World world) : NuciApiResponseContent
    {
        [HmacOrder(1)]
        public string Identifier { get; set; } = world.Identifier;

        [HmacOrder(2)]
        public LocalisedString Name { get; set; } = world.Name;

        [HmacOrder(3)]
        public bool HasWebMap { get; set; } = world.HasWebMap;

        [HmacOrder(4)]
        public Coordinates SpawnPoint { get; set; } = world.SpawnPoint;

        [HmacOrder(5)]
        public WorldType Type { get; set; } = world.Type;
    }
}
