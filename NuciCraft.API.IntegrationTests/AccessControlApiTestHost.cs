using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

using NuciDAL.Repositories;
using NuciLog.Core;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service;

namespace NuciCraft.API.IntegrationTests
{
    internal sealed class AccessControlApiTestHost : WebApplicationFactory<Program>
    {
        private static string ApiKey => "NucileRullz!";

        private static string ServerName => "NuciCraft";

        private static string StoreDirectoryName => "nucicraft-api-access-control-integration-tests";

        private readonly StubHttpMessageHandler httpMessageHandler;
        private readonly FakeTimeProvider timeProvider;
        private readonly InMemoryRepository<PlayerDataObject> playerRepository = new();
        private readonly bool deletesStoreDirectory;
        private readonly bool isRegisteredPlayerExemptionEnabled;
        private readonly string storeDirectory;

        internal AccessControlApiTestHost(StubHttpMessageHandler httpMessageHandler)
            : this(
                httpMessageHandler,
                new FakeTimeProvider(
                    new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero)),
                Path.Combine(
                    Path.GetTempPath(),
                    StoreDirectoryName,
                    Guid.NewGuid().ToString("N")),
                true,
                true)
        {
        }

        internal AccessControlApiTestHost(
            StubHttpMessageHandler httpMessageHandler,
            bool isRegisteredPlayerExemptionEnabled)
            : this(
                httpMessageHandler,
                new FakeTimeProvider(
                    new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero)),
                Path.Combine(
                    Path.GetTempPath(),
                    StoreDirectoryName,
                    Guid.NewGuid().ToString("N")),
                true,
                isRegisteredPlayerExemptionEnabled)
        {
        }

        private AccessControlApiTestHost(
            StubHttpMessageHandler httpMessageHandler,
            FakeTimeProvider timeProvider,
            string storeDirectory,
            bool deletesStoreDirectory,
            bool isRegisteredPlayerExemptionEnabled)
        {
            this.httpMessageHandler = httpMessageHandler;
            this.timeProvider = timeProvider;
            this.storeDirectory = storeDirectory;
            this.deletesStoreDirectory = deletesStoreDirectory;
            this.isRegisteredPlayerExemptionEnabled = isRegisteredPlayerExemptionEnabled;
        }

        internal FakeTimeProvider TimeProvider => timeProvider;

        internal InMemoryRepository<PlayerDataObject> PlayerRepository => playerRepository;

        internal HttpClient CreateAuthorisedClient()
        {
            HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTesting");
            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                Dictionary<string, string?> configurationValues = new()
                {
                    ["accessControlSettings:geolocationApiUrl"] = "https://geo.test/",
                    ["accessControlSettings:deniedUsernamePatterns:0"] = "^[Nn]uci[Ss]canner$",
                    ["accessControlSettings:minimumUsernameLength"] = "4",
                    ["accessControlSettings:anonymisationServiceDetection:provider"] = "funkemunky",
                    ["accessControlSettings:anonymisationServiceDetection:isEnabled"] = "true",
                    ["accessControlSettings:anonymisationServiceDetection:isCacheEnabled"] = "true",
                    ["accessControlSettings:isRegisteredPlayerExemptionEnabled"] = isRegisteredPlayerExemptionEnabled.ToString().ToLowerInvariant(),
                    ["dataStoreSettings:countriesStorePath"] = GetStorePath("countries.json"),
                    ["dataStoreSettings:homesStorePath"] = GetStorePath("homes.json"),
                    ["dataStoreSettings:playersStorePath"] = GetStorePath("players.json"),
                    ["dataStoreSettings:rtpLocationsStorePath"] = GetStorePath("rtp_locations.json"),
                    ["dataStoreSettings:worldsStorePath"] = GetStorePath("worlds.json"),
                    ["dataStoreSettings:zonesStorePath"] = GetStorePath("zones.json"),
                    ["dataStoreSettings:zoneTypesStorePath"] = GetStorePath("zone_types.json"),
                    ["securitySettings:apiKey"] = ApiKey,
                    ["serverSettings:name"] = ServerName,
                    ["nuciLoggerSettings:logFilePath"] = GetStorePath("nucicraft-api.log"),
                    ["nuciLoggerSettings:isFileOutputEnabled"] = "false"
                };

                configurationBuilder.AddInMemoryCollection(configurationValues);
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFileRepository<PlayerDataObject>>();
                services.AddSingleton<IFileRepository<PlayerDataObject>>(_ => playerRepository);

                services.RemoveAll<IFileRepository<CountryDataObject>>();
                services.AddSingleton<IFileRepository<CountryDataObject>>(_ => new InMemoryRepository<CountryDataObject>());

                services.RemoveAll<IFileRepository<HomeDataObject>>();
                services.AddSingleton<IFileRepository<HomeDataObject>>(_ => new InMemoryRepository<HomeDataObject>());

                services.RemoveAll<IFileRepository<ItemDataObject>>();
                services.AddSingleton<IFileRepository<ItemDataObject>>(_ => new InMemoryRepository<ItemDataObject>());

                services.RemoveAll<IFileRepository<WorldDataObject>>();
                services.AddSingleton<IFileRepository<WorldDataObject>>(_ => new InMemoryRepository<WorldDataObject>());

                services.RemoveAll<IFileRepository<ZoneDataObject>>();
                services.AddSingleton<IFileRepository<ZoneDataObject>>(_ => new InMemoryRepository<ZoneDataObject>());

                services.RemoveAll<IFileRepository<ZoneTypeDataObject>>();
                services.AddSingleton<IFileRepository<ZoneTypeDataObject>>(_ => new InMemoryRepository<ZoneTypeDataObject>());

                services.RemoveAll<HttpClient>();
                services.AddSingleton(_ => new HttpClient(httpMessageHandler));

                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(timeProvider);

                services.RemoveAll<ILogger>();
                services.AddSingleton<ILogger, NullLogger>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && deletesStoreDirectory && Directory.Exists(storeDirectory))
            {
                Directory.Delete(storeDirectory, true);
            }
        }

        private string GetStorePath(string fileName) => Path.Combine(storeDirectory, fileName);
    }
}
