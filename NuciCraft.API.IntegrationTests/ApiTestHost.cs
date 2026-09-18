using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using NuciAPI.Client;
using NuciAPI.Responses;

using NuciCraft.API.Service;

namespace NuciCraft.API.IntegrationTests
{
    internal sealed class ApiTestHost : WebApplicationFactory<Program>
    {
        private static string ApiKey => "NucileRullz!";

        private static string StoreDirectoryName => "nucicraft-api-integration-tests";

        private readonly string storeDirectory = Path.Combine(
            Path.GetTempPath(),
            StoreDirectoryName,
            Guid.NewGuid().ToString("N"));

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
                    ["dataStoreSettings:countriesStorePath"] = GetStorePath("countries.json"),
                    ["dataStoreSettings:playersStorePath"] = GetStorePath("players.json"),
                    ["dataStoreSettings:rtpLocationsStorePath"] = GetStorePath("rtp_locations.json"),
                    ["dataStoreSettings:worldsStorePath"] = GetStorePath("worlds.json"),
                    ["dataStoreSettings:zonesStorePath"] = GetStorePath("zones.json"),
                    ["dataStoreSettings:zoneTypesStorePath"] = GetStorePath("zone_types.json"),
                    ["rtpLocationSettings:minimumLocationDistance"] = "613",
                    ["rtpLocationSettings:minimumBiomeLocationDistance"] = "873",
                    ["webMapSettings:baseUrl"] = "https://example.invalid/webmap/",
                    ["securitySettings:apiKey"] = ApiKey,
                    ["universalNameGeneratorSettings:baseUrl"] = "https://example.invalid",
                    ["universalNameGeneratorSettings:apiKey"] = ApiKey,
                    ["nuciLoggerSettings:logFilePath"] = GetStorePath("nucicraft-api.log"),
                    ["nuciLoggerSettings:isFileOutputEnabled"] = "false"
                };

                configurationBuilder.AddInMemoryCollection(configurationValues);
            });
            builder.ConfigureServices(services =>
            {
                Mock<INuciApiClient> nameGeneratorClientMock = new();
                nameGeneratorClientMock
                    .Setup(client => client.SendRequestAsync<
                        GenerateNamesRequest,
                        NuciApiContentResponse<GenerateNamesResponse>>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                    .ReturnsAsync(new NuciApiContentResponse<GenerateNamesResponse>(new()
                    {
                        Names = ["Ilarion"]
                    }));
                services.RemoveAll<INuciApiClient>();
                services.AddSingleton(nameGeneratorClientMock.Object);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && Directory.Exists(storeDirectory))
            {
                Directory.Delete(storeDirectory, true);
            }
        }

        private string GetStorePath(string fileName) => Path.Combine(storeDirectory, fileName);
    }
}