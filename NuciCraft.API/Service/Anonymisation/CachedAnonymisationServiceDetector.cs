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
        TimeProvider timeProvider,
        ILogger logger) : IAnonymisationServiceDetector
    {
        private static string CacheKeyPrefix => "anonymisation:";

        private readonly IAnonymisationServiceDetector innerDetector = innerDetector;
        private readonly IMemoryCache memoryCache = memoryCache;
        private readonly AccessControlSettings settings = settings;
        private readonly TimeProvider timeProvider = timeProvider;
        private readonly ILogger logger = logger;

        public async Task<bool> IsAnonymisationService(string ipAddress, CancellationToken cancellationToken)
        {
            AnonymisationServiceDetectionSettings detectionSettings = settings.AnonymisationServiceDetection;

            if (!detectionSettings.IsCacheEnabled)
            {
                return await innerDetector.IsAnonymisationService(ipAddress, cancellationToken);
            }

            string cacheKey = $"{CacheKeyPrefix}{ipAddress}";
            memoryCache.TryGetValue(cacheKey, out AnonymisationDetectionCacheEntry? cachedEntry);

            if (cachedEntry is not null && cachedEntry.ExpiresAt > timeProvider.GetUtcNow())
            {
                return cachedEntry.IsAnonymisationService;
            }

            try
            {
                bool result = await innerDetector.IsAnonymisationService(ipAddress, cancellationToken);

                CacheResult(cacheKey, result, detectionSettings);

                return result;
            }
            catch (Exception exception)
            {
                if (cachedEntry is null)
                {
                    throw;
                }

                logger.Warn(
                    MyOperation.CheckAnonymisationService,
                    OperationStatus.Failure,
                    exception,
                    [new(MyLogInfoKey.IpAddress, ipAddress)]);

                return cachedEntry.IsAnonymisationService;
            }
        }

        private void CacheResult(
            string cacheKey,
            bool result,
            AnonymisationServiceDetectionSettings detectionSettings)
        {
            TimeSpan cacheDuration;
            if (result)
            {
                cacheDuration = TimeSpan.FromHours(detectionSettings.CacheDurationHoursWhenDetected);
            }
            else
            {
                cacheDuration = TimeSpan.FromHours(detectionSettings.CacheDurationHoursWhenNotDetected);
            }

            AnonymisationDetectionCacheEntry entry = new()
            {
                IsAnonymisationService = result,
                ExpiresAt = timeProvider.GetUtcNow().Add(cacheDuration)
            };

            memoryCache.Remove(cacheKey);
            memoryCache.Set(cacheKey, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration + GetFallbackRetention(detectionSettings)
            });
        }

        private static TimeSpan GetFallbackRetention(
            AnonymisationServiceDetectionSettings detectionSettings)
            => TimeSpan.FromHours(detectionSettings.CacheDurationHoursForExpiredFallback);
    }
}