using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;

using Moq;

using NuciLog.Core;

using NUnit.Framework;

using NuciCraft.API.Configuration;
using NuciCraft.API.Service.Anonymisation;

namespace NuciCraft.API.UnitTests.Service.Anonymisation
{
    [TestFixture]
    public sealed class CachedAnonymisationServiceDetectorTests
    {
        private static string IpAddress => "8.8.8.8";

        private FakeTimeProvider timeProvider;
        private MemoryCache memoryCache;
        private Mock<ILogger> logger;
        private Mock<IAnonymisationServiceDetector> innerDetectorMock;
        private AccessControlSettings settings;
        private CachedAnonymisationServiceDetector detector;

        [SetUp]
        public void SetUp()
        {
            timeProvider = new FakeTimeProvider(
                new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            logger = new Mock<ILogger>();
            innerDetectorMock = new Mock<IAnonymisationServiceDetector>();
            settings = new AccessControlSettings
            {
                AnonymisationServiceDetection = new()
                {
                    Provider = "funkemunky",
                    IsEnabled = true,
                    IsCacheEnabled = true,
                    CacheDurationHoursWhenDetected = 6,
                    CacheDurationHoursWhenNotDetected = 2
                }
            };

            detector = new CachedAnonymisationServiceDetector(
                innerDetectorMock.Object,
                memoryCache,
                settings,
                timeProvider,
                logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            memoryCache.Dispose();
        }

        [Test]
        public async Task GivenACachedResult_WhenCheckingTheIp_ThenTheCachedResultIsReturnedWithoutALiveRequest()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            bool result = await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(result, Is.True);
            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GivenNoCachedResult_WhenCheckingTheIp_ThenTheLiveRequestIsMade()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            bool result = await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(result, Is.False);
            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    IpAddress,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GivenNoAnonymisationServiceDetected_WhenCheckingTheIp_ThenTheResultIsCachedForTwoHours()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            AnonymisationDetectionCacheEntry? entry = GetCachedEntry(IpAddress);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.IsAnonymisationService, Is.False);
            Assert.That(
                entry.ExpiresAt,
                Is.EqualTo(timeProvider.GetUtcNow().AddHours(2)));
        }

        [Test]
        public async Task GivenAnAnonymisationServiceDetected_WhenCheckingTheIp_ThenTheResultIsCachedForSixHours()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            AnonymisationDetectionCacheEntry? entry = GetCachedEntry(IpAddress);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.IsAnonymisationService, Is.True);
            Assert.That(
                entry.ExpiresAt,
                Is.EqualTo(timeProvider.GetUtcNow().AddHours(6)));
        }

        [Test]
        public async Task GivenACachedResultWithinItsLifetime_WhenCheckingTheIp_ThenTheLiveRequestIsNotMade()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromMinutes(119));

            bool result = await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(result, Is.False);
            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    IpAddress,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GivenAnExpiredCachedResult_WhenCheckingTheIp_ThenTheLiveRequestIsMade()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromHours(2));

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    IpAddress,
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task GivenAnExpiredCachedResultAndASuccessfulLiveRequest_WhenCheckingTheIp_ThenTheNewResultReplacesTheOldCache()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromHours(2));
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            bool result = await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(result, Is.True);
            AnonymisationDetectionCacheEntry? entry = GetCachedEntry(IpAddress);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.IsAnonymisationService, Is.True);
            Assert.That(
                entry.ExpiresAt,
                Is.EqualTo(timeProvider.GetUtcNow().AddHours(6)));
        }

        [Test]
        public async Task GivenAnExpiredCachedResultAndAFailedLiveRequest_WhenCheckingTheIp_ThenTheExpiredResultIsUsedAsFallback()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromHours(6));
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("The detection service is unavailable."));

            bool result = await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GivenAnExpiredCachedResultAndAFailedLiveRequest_WhenCheckingTheIp_ThenTheFailureIsLogged()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromHours(6));
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("The detection service is unavailable."));

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(
                logger.Invocations.Count(invocation => invocation.Method.Name == nameof(ILogger.Warn)),
                Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task GivenAnExpiredCachedResultAndAFailedLiveRequest_WhenCheckingTheIp_ThenTheExpiredCacheIsRetained()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            timeProvider.Advance(TimeSpan.FromHours(6));
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("The detection service is unavailable."));

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            AnonymisationDetectionCacheEntry? entry = GetCachedEntry(IpAddress);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.IsAnonymisationService, Is.True);
        }

        [Test]
        public async Task GivenNoCachedResultAndAFailedLiveRequest_WhenCheckingTheIp_ThenTheExceptionIsThrown()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("The detection service is unavailable."));

            Assert.That(
                () => detector.IsAnonymisationService(IpAddress, CancellationToken.None),
                Throws.InstanceOf<HttpRequestException>());
        }

        [Test]
        public async Task GivenADisabledCache_WhenCheckingTheIp_ThenTheLiveRequestIsMadeOnEveryCheck()
        {
            settings.AnonymisationServiceDetection.IsCacheEnabled = false;
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);
            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    IpAddress,
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            Assert.That(GetCachedEntry(IpAddress), Is.Null);
        }

        [Test]
        public async Task GivenADisabledCache_WhenCheckingTheIp_ThenNoCacheEntryIsStored()
        {
            settings.AnonymisationServiceDetection.IsCacheEnabled = false;
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await detector.IsAnonymisationService(IpAddress, CancellationToken.None);

            Assert.That(GetCachedEntry(IpAddress), Is.Null);
        }

        [Test]
        public async Task GivenDifferentIpAddresses_WhenCheckingTheIp_ThenEachAddressIsCachedSeparately()
        {
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await detector.IsAnonymisationService("8.8.8.8", CancellationToken.None);

            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            bool result = await detector.IsAnonymisationService("1.1.1.1", CancellationToken.None);

            Assert.That(result, Is.False);
            Assert.That(GetCachedEntry("8.8.8.8")!.IsAnonymisationService, Is.True);
            Assert.That(GetCachedEntry("1.1.1.1")!.IsAnonymisationService, Is.False);
        }

        [Test]
        public async Task GivenACancelledToken_WhenCheckingTheIp_ThenTheTokenIsPassedToTheLiveRequest()
        {
            using CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            innerDetectorMock
                .Setup(detectorMock => detectorMock.IsAnonymisationService(
                    It.IsAny<string>(),
                    cancellationToken))
                .ReturnsAsync(false);

            await detector.IsAnonymisationService(IpAddress, cancellationToken);

            innerDetectorMock.Verify(
                detectorMock => detectorMock.IsAnonymisationService(
                    IpAddress,
                    cancellationToken),
                Times.Once);
        }

        private AnonymisationDetectionCacheEntry? GetCachedEntry(string ipAddress)
        {
            memoryCache.TryGetValue(
                $"anonymisation:{ipAddress}",
                out AnonymisationDetectionCacheEntry? entry);

            return entry;
        }
    }
}
