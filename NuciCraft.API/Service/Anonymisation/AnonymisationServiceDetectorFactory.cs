using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.Service.Anonymisation
{
    public sealed class AnonymisationServiceDetectorFactory(
        AccessControlSettings settings,
        IServiceProvider serviceProvider) : IAnonymisationServiceDetectorFactory
    {
        private readonly AccessControlSettings settings = settings;
        private readonly IServiceProvider serviceProvider = serviceProvider;

        public IAnonymisationServiceDetector Create()
        {
            string provider = settings.AnonymisationServiceDetection.Provider;

            HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();

            if (string.Equals(provider, "proxycheck", StringComparison.OrdinalIgnoreCase))
            {
                return new ProxyCheckAnonymisationServiceDetector(settings, httpClient);
            }

            if (string.Equals(provider, "funkemunky", StringComparison.OrdinalIgnoreCase))
            {
                return new FunkeMunkyAnonymisationServiceDetector(settings, httpClient);
            }

            throw new NotSupportedException($"Unsupported anonymisation service provider '{provider}'.");
        }
    }
}
