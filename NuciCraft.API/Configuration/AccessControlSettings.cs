namespace NuciCraft.API.Configuration
{
    public sealed class AccessControlSettings
    {
        public string[] DeniedCountries { get; set; } = [];

        public string[] DeniedUsernamePatterns { get; set; } = [];

        public string[] DeniedExactUsernames { get; set; } = [];

        public int MinimumUsernameLength { get; set; } = 4;
    }
}