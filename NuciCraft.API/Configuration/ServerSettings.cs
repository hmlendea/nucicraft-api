namespace NuciCraft.API.Configuration
{
    public sealed class ServerSettings
    {
        public string Name { get; set; } = string.Empty;

        public string Hostname { get; set; } = string.Empty;

        public int JavaEditionPort { get; set; }

        public int BedrockEditionPort { get; set; }
    }
}