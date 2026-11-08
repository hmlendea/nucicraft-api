using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using NuciLog.Core;

using NuciCraft.API.Configuration;
using NuciCraft.API.Logging;

namespace NuciCraft.API.Service.Anonymisation
{
    public sealed class CachedAnonymisationServiceDetector(
        IAnonymisationServiceDetector innerDetector,
        IMemoryCache memoryCache,
        AccessControlSettings settings,
        ILogger logger) : IAnonymisationServiceDetector
    {
        private readonly IAnonymisationServiceDetector innerDetector = innerDetector;
        private readonly IMemoryCache memoryCache = memoryCache;
        private readonly AccessControlSettings settings = settings;
        private readonly ILogger logger = logger;

        public async Task<bool> IsAnonymisationService(string ipAddress, CancellationToken cancellationToken)
        {
            string cacheKey = $"anonymisation:{ipAddress}";

            if (memoryCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            AnonymisationServiceDetectionSettings detectionSettings = settings.AnonymisationServiceDetection;

            if (!detectionSettings.IsCacheEnabled)
            {
                return await innerDetector.IsAnonymisationService(ipAddress, cancellationToken);
            }

            try
            {
                bool result = await innerDetector.IsAnonymisationService(ipAddress, cancellationToken);

                TimeSpan cacheDuration;
                if (result)
                {
                    cacheDuration = TimeSpan.FromHours(detectionSettings.CacheDurationHoursWhenDetected);
                }
                else
                {
                    cacheDuration = TimeSpan.FromHours(detectionSettings.CacheDurationHoursWhenNotDetected);
                }

                memoryCache.Set(cacheKey, result, cacheDuration);

                return result;
            }
            catch (Exception exception)
            {
                if (memoryCache.TryGetValue(cacheKey, out bool expiredResult))
                {
                    logger.Warn(
                        MyOperation.CheckIp,
                        OperationStatus.Failure,
                        exception,
                        [new(MyLogInfoKey.IpAddress, ipAddress)]);

                    return expiredResult;
                }

                throw;
            }
        }
    }
}