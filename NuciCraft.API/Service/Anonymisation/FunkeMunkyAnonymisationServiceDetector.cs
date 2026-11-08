using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service.Models;

using NuciWeb.HTTP;

namespace NuciCraft.API.Service.Anonymisation
{
    public sealed class FunkeMunkyAnonymisationServiceDetector(
        AccessControlSettings settings,
        HttpClient httpClient) : IAnonymisationServiceDetector
    {
        private readonly AccessControlSettings settings = settings;
        private readonly HttpClient httpClient = httpClient;

        public FunkeMunkyAnonymisationServiceDetector(AccessControlSettings settings)
            : this(settings, HttpClientCreator.Create())
        {
        }

        public async Task<bool> IsAnonymisationService(string ipAddress, CancellationToken cancellationToken)
        {
            string url = $"https://funkemunky.cc/vpn?ip={Uri.EscapeDataString(ipAddress)}&license=none&cache=false";

            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            AnonymisationServiceCheckData? anonymisationServiceCheck = JsonSerializer.Deserialize<AnonymisationServiceCheckData>(json);

            return anonymisationServiceCheck is not null && anonymisationServiceCheck.Success && anonymisationServiceCheck.IsAnonymisationService;
        }
    }
}
