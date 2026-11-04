namespace NuciCraft.API.Configuration
{
    public sealed class AccessControlSettings
    {
        public string[] DeniedCountries { get; set; } = [];

        public string[] DeniedUsernamePatterns { get; set; } = [];

        public string GeolocationApiUrl { get; set; } = "http://ip-api.com/json/";

        public string AnonymisationServiceDetectionApiUrl { get; set; } = "https://funkemunky.cc/vpn";

        public string AnonymisationServiceDetectionApiLicense { get; set; } = "none";

        public bool IsAnonymisationServiceDetectionEnabled { get; set; } = true;

        public int MinimumUsernameLength { get; set; } = 4;
    }
}