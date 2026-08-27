using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.UnitTests
{
    internal static class TestConfigurationFactory
    {
        private static string ApiKey => "NucileRullz!";

        private static string CountriesStoreFileName => "countries.json";

        private static string LogFileName => "nucicraft-api.log";

        private static string PlayersStoreFileName => "players.json";

        private static string RtpLocationsStoreFileName => "rtp_locations.json";

        private static string WorldsStoreFileName => "worlds.json";

        private static string ZonesStoreFileName => "zones.json";

        internal static IConfiguration Build(string storeDirectory)
        {
            DataStoreSettings dataStoreSettings = BuildDataStoreSettings(storeDirectory);
            IEnumerable<KeyValuePair<string, string>> values =
            [
                new("dataStoreSettings:countriesStorePath", dataStoreSettings.CountriesStorePath),
                new("dataStoreSettings:worldsStorePath", dataStoreSettings.WorldsStorePath),
                new("dataStoreSettings:playersStorePath", dataStoreSettings.PlayersStorePath),
                new("dataStoreSettings:rtpLocationsStorePath", dataStoreSettings.RtpLocationsStorePath),
                new("dataStoreSettings:zonesStorePath", dataStoreSettings.ZonesStorePath),
                new("rtpLocationSettings:minimumLocationDistance", "613"),
                new("rtpLocationSettings:minimumBiomeLocationDistance", "873"),
                new("securitySettings:apiKey", ApiKey),
                new("universalNameGeneratorSettings:baseUrl", "https://dummy-url.com"),
                new("universalNameGeneratorSettings:apiKey", ApiKey),
                new("nuciLoggerSettings:logFilePath", Path.Combine(storeDirectory, LogFileName)),
                new("nuciLoggerSettings:isFileOutputEnabled", "false"),
            ];

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        internal static string[] BuildCommandLineArguments(string storeDirectory)
        {
            DataStoreSettings dataStoreSettings = BuildDataStoreSettings(storeDirectory);

            return
            [
                "--dataStoreSettings:countriesStorePath",
                dataStoreSettings.CountriesStorePath,
                "--dataStoreSettings:worldsStorePath",
                dataStoreSettings.WorldsStorePath,
                "--dataStoreSettings:playersStorePath",
                dataStoreSettings.PlayersStorePath,
                "--dataStoreSettings:rtpLocationsStorePath",
                dataStoreSettings.RtpLocationsStorePath,
                "--dataStoreSettings:zonesStorePath",
                dataStoreSettings.ZonesStorePath,
                "--universalNameGeneratorSettings:baseUrl",
                "https://dummy-url.com",
                "--universalNameGeneratorSettings:apiKey",
                ApiKey,
                "--nuciLoggerSettings:logFilePath",
                Path.Combine(storeDirectory, LogFileName),
                "--nuciLoggerSettings:isFileOutputEnabled",
                "false",
            ];
        }

        internal static DataStoreSettings BuildDataStoreSettings(string storeDirectory) => new()
        {
            CountriesStorePath = Path.Combine(storeDirectory, CountriesStoreFileName),
            WorldsStorePath = Path.Combine(storeDirectory, WorldsStoreFileName),
            PlayersStorePath = Path.Combine(storeDirectory, PlayersStoreFileName),
            RtpLocationsStorePath = Path.Combine(storeDirectory, RtpLocationsStoreFileName),
            ZonesStorePath = Path.Combine(storeDirectory, ZonesStoreFileName)
        };
    }
}