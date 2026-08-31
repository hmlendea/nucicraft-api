namespace NuciCraft.API.Service.Models
{
    public class World
    {
        public string Identifier { get; set; }

        public LocalisedString Name { get; set; }

        public bool HasWebMap { get; set; }

        public Coordinates SpawnPoint { get; set; }

        public WorldType Type { get; set; } = WorldType.Overworld;
    }
}
