namespace NuciCraft.API.Configuration
{
    public sealed class AnonymisationServiceDetectionSettings
    {
        public string Provider { get; set; } = "proxycheck";

        public bool IsEnabled { get; set; } = true;

        public bool IsCacheEnabled { get; set; } = true;
    }
}
