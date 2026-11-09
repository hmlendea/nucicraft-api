using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using NuciAPI.Client;
using NuciDAL.Repositories;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.IntegrationTests
{
    internal sealed class ApiTestHost : WebApplicationFactory<Program>
    {
        private static string ApiKey => "NucileRullz!";

        private static string StoreDirectoryName => "nucicraft-api-integration-tests";

        private readonly bool deletesStoreDirectory;
        private readonly string storeDirectory;
        private readonly bool useInMemoryRepositories;

        public ApiTestHost()
            : this(
                Path.Combine(
                    Path.GetTempPath(),
                    StoreDirectoryName,
                    Guid.NewGuid().ToString("N")),
                true,
                true)
        {
        }

        public ApiTestHost(string storeDirectory)
            : this(storeDirectory, false, false)
        {
        }

        private ApiTestHost(
            string storeDirectory,
            bool deletesStoreDirectory,
            bool useInMemoryRepositories)
        {
            this.storeDirectory = storeDirectory;
            this.deletesStoreDirectory = deletesStoreDirectory;
            this.useInMemoryRepositories = useInMemoryRepositories;
        }

        public HttpClient CreateAuthorisedClient()
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
                    ["rtpLocationSettings:minimumLocationDistance"] = "613",
                    ["rtpLocationSettings:minimumBiomeLocationDistance"] = "873",
                    ["webMapSettings:baseUrl"] = "https://example.invalid/webmap/",
                    ["serverSettings:name"] = "Testy McTestface",
                    ["serverSettings:hostname"] = "example.invalid",
                    ["serverSettings:javaEditionPort"] = "613",
                    ["serverSettings:bedrockEditionPort"] = "873",
                    ["securitySettings:apiKey"] = ApiKey,
                    ["universalNameGeneratorSettings:baseUrl"] = "https://example.invalid",
                    ["universalNameGeneratorSettings:apiKey"] = ApiKey,
                    ["nuciLoggerSettings:logFilePath"] = GetStorePath("nucicraft-api.log"),
                    ["nuciLoggerSettings:isFileOutputEnabled"] = "false",
                    ["dataStoreSettings:countriesStorePath"] = GetStorePath("countries.json"),
                    ["dataStoreSettings:homesStorePath"] = GetStorePath("homes.json"),
                    ["dataStoreSettings:itemsStorePath"] = GetStorePath("items.json"),
                    ["dataStoreSettings:playersStorePath"] = GetStorePath("players.json"),
                    ["dataStoreSettings:rtpLocationsStorePath"] = GetStorePath("rtp_locations.json"),
                    ["dataStoreSettings:worldsStorePath"] = GetStorePath("worlds.json"),
                    ["dataStoreSettings:zonesStorePath"] = GetStorePath("zones.json"),
                    ["dataStoreSettings:zoneTypesStorePath"] = GetStorePath("zone_types.json")
                };

                configurationBuilder.AddInMemoryCollection(configurationValues);
            });
            builder.ConfigureServices(services =>
            {
                if (useInMemoryRepositories)
                {
                    services.RemoveAll<IFileRepository<PlayerDataObject>>();
                    services.AddSingleton<IFileRepository<PlayerDataObject>>(_ => new InMemoryRepository<PlayerDataObject>());

                    services.RemoveAll<IFileRepository<RtpLocationEntity>>();
                    services.AddSingleton<IFileRepository<RtpLocationEntity>>(_ => new InMemoryRepository<RtpLocationEntity>());

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
                }

                Mock<IAccessControlService> accessControlServiceMock = new();
                accessControlServiceMock
                    .Setup(service => service.CheckIpAsync(It.IsAny<CheckIpRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CheckIpRequest request, CancellationToken _) => request.IpAddress switch
                    {
                        "8.8.8.8" => new IpCheckResult
                        {
                            IsAllowed = false,
                            CountryCode = "CN",
                            DeniedReason = "Access is not permitted from this country."
                        },
                        "1.1.1.1" => new IpCheckResult
                        {
                            IsAllowed = false,
                            CountryCode = "US",
                            DeniedReason = "Access is not permitted via any anonymisation service."
                        },
                        "9.9.9.9" => new IpCheckResult
                        {
                            IsAllowed = true,
                            CountryCode = "US",
                            DeniedReason = null
                        },
                        _ => new IpCheckResult
                        {
                            IsAllowed = true,
                            CountryCode = null,
                            DeniedReason = null
                        }
                    });
                accessControlServiceMock
                    .Setup(service => service.CheckUsernameAsync(It.IsAny<CheckUsernameRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CheckUsernameRequest request, CancellationToken _) =>
                    {
                        if (request.Username.Contains("player", StringComparison.OrdinalIgnoreCase))
                        {
                            return new UsernameCheckResult
                            {
                                IsAllowed = false,
                                DeniedReason = "This username is not permitted."
                            };
                        }

                        return new UsernameCheckResult
                        {
                            IsAllowed = true,
                            DeniedReason = null
                        };
                    });
                services.RemoveAll<IAccessControlService>();
                services.AddTransient(_ => accessControlServiceMock.Object);

                Mock<INuciApiClient> nameGeneratorClientMock = new();
                nameGeneratorClientMock
                    .Setup(client => client.SendRequestAsync<
                        GenerateNamesRequest,
                        GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                    .ReturnsAsync((
                        HttpMethod method,
                        GenerateNamesRequest request,
                        NuciApiRequestAuthorisationInfo authorisationInfo,
                        string endpoint) =>
                            new GenerateNamesResponse
                            {
                                Names = Enumerable.Repeat("Ilarion", request.Count)
                            });
                services.RemoveAll<INuciApiClient>();
                services.AddSingleton(nameGeneratorClientMock.Object);
                Mock<IServerStatusService> serverStatusServiceMock = new();
                serverStatusServiceMock
                    .Setup(service => service.GetOnlinePlayersCountAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(42);
                services.RemoveAll<IServerStatusService>();
                services.AddSingleton(serverStatusServiceMock.Object);
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
