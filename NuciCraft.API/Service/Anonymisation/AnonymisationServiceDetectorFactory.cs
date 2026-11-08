using System;
using System.Net.Http;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using NuciLog.Core;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;

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
            IMemoryCache memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            ILogger logger = serviceProvider.GetRequiredService<ILogger>();

            IAnonymisationServiceDetector innerDetector;

            if (string.Equals(provider, "proxycheck", StringComparison.OrdinalIgnoreCase))
            {
                innerDetector = new ProxyCheckAnonymisationServiceDetector(settings, httpClient);
            }
            else if (string.Equals(provider, "funkemunky", StringComparison.OrdinalIgnoreCase))
            {
                innerDetector = new FunkeMunkyAnonymisationServiceDetector(settings, httpClient);
            }
            else
            {
                throw new NotSupportedException($"Unsupported anonymisation service provider '{provider}'.");
            }

            return new CachedAnonymisationServiceDetector(innerDetector, memoryCache, settings, logger);
        }
    }
}
