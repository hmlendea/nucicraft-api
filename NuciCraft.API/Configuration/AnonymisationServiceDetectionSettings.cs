namespace NuciCraft.API.Configuration
{
    public sealed class AnonymisationServiceDetectionSettings
    {
        public string ApiUrl { get; set; } = "https://funkemunky.cc/vpn";

        public string ApiLicense { get; set; } = "none";

        public bool IsEnabled { get; set; } = true;

        public bool IsCacheEnabled { get; set; } = true;
    }
}
