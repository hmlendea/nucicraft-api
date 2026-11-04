namespace NuciCraft.API.Configuration
{
    public sealed class AccessControlSettings
    {
        public string[] DeniedCountries { get; set; } = [];

        public string[] DeniedUsernamePatterns { get; set; } = [];

        public string GeolocationApiUrl { get; set; } = "http://ip-api.com/json/";

        public AnonymisationServiceDetectionSettings AnonymisationServiceDetection { get; set; } = new();

        public int MinimumUsernameLength { get; set; } = 4;
    }
}