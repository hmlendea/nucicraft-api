using System;
using System.Net.Http;

using Moq;
using NUnit.Framework;

using NuciAPI.Client;
using NuciAPI.Responses;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;

using NuciLog.Core;

namespace NuciCraft.API.UnitTests.Service
{
    [TestFixture]
    public class MobServiceTests
    {
        private Mock<INuciApiClient> universalNameGeneratorClientMock;
        private Mock<ILogger> loggerMock;
        private UniversalNameGeneratorSettings settings;
        private MobService mobService;

        [SetUp]
        public void SetUp()
        {
            universalNameGeneratorClientMock = new Mock<INuciApiClient>();
            loggerMock = new Mock<ILogger>();

            settings = new()
            {
                BaseUrl = "https://name-generator.nucilandia.ro",
                ApiKey = "test-api-key"
            };

            mobService = new MobService(
                universalNameGeneratorClientMock.Object,
                settings,
                loggerMock.Object);
        }

        [Test]
        public void GivenASupportedMobType_WhenGettingARandomMobName_ThenTheGeneratedNameIsReturned()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        "Names"))
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = ["Ilarion"]
                });

            string generatedName = mobService.GetRandomMobName(BuildGetMobNameRequest());

            Assert.That(generatedName, Is.EqualTo("Ilarion"));
        }

        [Test]
        public void GivenASupportedMobType_WhenGettingARandomMobName_ThenTheUniversalNameGeneratorRequestIsBuiltCorrectly()
        {
            GenerateNamesRequest capturedRequest = null;

            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        "Names"))
                .Callback<HttpMethod, GenerateNamesRequest, string>(
                    (
                        method,
                        request,
                        endpoint) => capturedRequest = request)
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = ["Ilarion"]
                });

            mobService.GetRandomMobName(BuildGetMobNameRequest());

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.ApiKey, Is.EqualTo(settings.ApiKey));
            Assert.That(
                capturedRequest.Schema,
                Is.EqualTo("romanian-persons-male"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenAnUnsupportedMobType_WhenGettingARandomMobName_ThenANotImplementedExceptionIsThrown()
        {
            GetMobNameRequest request = BuildGetMobNameRequest();
            request.MobType = "zombie";

            Assert.That(
                () => mobService.GetRandomMobName(request),
                Throws.TypeOf<NotImplementedException>());

            universalNameGeneratorClientMock.Verify(
                client => client.SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                    HttpMethod.Get,
                    It.IsAny<GenerateNamesRequest>(),
                    "Names"),
                Times.Never);
        }

        [Test]
        public void GivenAnEmptyUniversalNameGeneratorResponse_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionIsThrown()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        "Names"))
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = []
                });

            Assert.That(
                () => mobService.GetRandomMobName(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAMissingBaseUrl_WhenGettingARandomMobName_ThenAnArgumentExceptionIsThrown()
        {
            settings.BaseUrl = string.Empty;

            Assert.That(
                () => mobService.GetRandomMobName(BuildGetMobNameRequest()),
                Throws.TypeOf<ArgumentException>());
        }

        private static GetMobNameRequest BuildGetMobNameRequest() => new()
        {
            MobType = MobType.WanderingTrader
        };
    }
}