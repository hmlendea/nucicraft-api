namespace NuciCraft.API.Configuration
{
    public sealed class AnonymisationServiceDetectionSettings
    {
        public string Provider { get; set; } = "proxycheck";

        public bool IsEnabled { get; set; } = true;

        public bool IsCacheEnabled { get; set; } = true;

        public int CacheDurationHoursWhenDetected { get; set; } = 6;

        public int CacheDurationHoursWhenNotDetected { get; set; } = 2;
    }
}
