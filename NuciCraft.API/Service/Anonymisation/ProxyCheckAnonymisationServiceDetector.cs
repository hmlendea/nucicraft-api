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
    public sealed class ProxyCheckAnonymisationServiceDetector(
        AccessControlSettings settings,
        HttpClient httpClient) : IAnonymisationServiceDetector
    {
        private static readonly JsonSerializerOptions ProxyCheckJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AccessControlSettings settings = settings;
        private readonly HttpClient httpClient = httpClient;

        public ProxyCheckAnonymisationServiceDetector(AccessControlSettings settings)
            : this(settings, HttpClientCreator.Create())
        {
        }

        public async Task<bool> IsAnonymisationService(string ipAddress, CancellationToken cancellationToken)
        {
            string url = $"https://proxycheck.io/v2/{Uri.EscapeDataString(ipAddress)}?vpn=1&asn=1&risk=1&cache={settings.AnonymisationServiceDetection.IsCacheEnabled.ToString().ToLowerInvariant()}";

            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            ProxyCheckResponseData? proxyCheck = JsonSerializer.Deserialize<ProxyCheckResponseData>(json, ProxyCheckJsonOptions);

            if (proxyCheck is null || proxyCheck.IpAddress is null)
            {
                return false;
            }

            return string.Equals(proxyCheck.Status, "ok", StringComparison.OrdinalIgnoreCase) &&
                proxyCheck.IpAddress.IsProxy;
        }
    }
}
