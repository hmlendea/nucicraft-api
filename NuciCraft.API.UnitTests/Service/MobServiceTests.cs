using System;
using System.Net.Http;
using System.Reflection;

using Moq;
using NUnit.Framework;

using NuciAPI.Client;
using NuciAPI.Responses;
using NuciSecurity.HMAC;

using NuciCraft.API.Configuration;
using NuciCraft.API.Requests;
using NuciCraft.API.Service;

using NuciLog.Core;
using NuciCraft.API.Service.Models;

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
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
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
            NuciApiRequestAuthorisationInfo capturedAuthorisationInfo = null;

            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .Callback<HttpMethod, GenerateNamesRequest, NuciApiRequestAuthorisationInfo, string>(
                    (
                        method,
                        request,
                        authorisationInfo,
                        endpoint) =>
                    {
                        capturedRequest = request;
                        capturedAuthorisationInfo = authorisationInfo;
                    })
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = ["Ilarion"]
                });

            mobService.GetRandomMobName(BuildGetMobNameRequest());

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedAuthorisationInfo, Is.Not.Null);
            Assert.That(
                capturedAuthorisationInfo.BearerToken,
                Is.EqualTo(settings.ApiKey));
            Assert.That(
                capturedRequest.Schema,
                Is.EqualTo("romanian-persons-male"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenTheEnderDragonMobType_WhenGettingARandomMobName_ThenTheFantasyDragonsSchemaIsUsed()
        {
            GenerateNamesRequest capturedRequest = null;

            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .Callback<HttpMethod, GenerateNamesRequest, NuciApiRequestAuthorisationInfo, string>(
                    (
                        method,
                        request,
                        authorisationInfo,
                        endpoint) => capturedRequest = request)
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = ["Smaug"]
                });

            mobService.GetRandomMobName(
                BuildGetMobNameRequest(MobType.EnderDragon));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Schema, Is.EqualTo("fantasy-dragons"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenTheVillageMobType_WhenGettingARandomMobName_ThenARomanianPersonSchemaIsUsed()
        {
            GenerateNamesRequest capturedRequest = null;

            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .Callback<HttpMethod, GenerateNamesRequest, NuciApiRequestAuthorisationInfo, string>(
                    (
                        method,
                        request,
                        authorisationInfo,
                        endpoint) => capturedRequest = request)
                .ReturnsAsync((NuciApiResponse)new GenerateNamesResponse
                {
                    Names = ["Mary"]
                });

            mobService.GetRandomMobName(
                BuildGetMobNameRequest(MobType.Village));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(
                capturedRequest.Schema,
                Is.EqualTo("romanian-persons-male")
                    .Or.EqualTo("romanian-persons-female"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenTheGenerateNamesRequest_WhenInspectingItsContract_ThenTheHmacOrderingMatchesTheUngApi()
        {
            PropertyInfo schemaProperty = typeof(GenerateNamesRequest)
                .GetProperty(nameof(GenerateNamesRequest.Schema));
            PropertyInfo countProperty = typeof(GenerateNamesRequest)
                .GetProperty(nameof(GenerateNamesRequest.Count));
            HmacOrderAttribute schemaAttribute = schemaProperty
                .GetCustomAttribute<HmacOrderAttribute>();
            HmacOrderAttribute countAttribute = countProperty
                .GetCustomAttribute<HmacOrderAttribute>();

            Assert.That(schemaAttribute, Is.Not.Null);
            Assert.That(schemaAttribute.Order, Is.EqualTo(1));
            Assert.That(countAttribute, Is.Not.Null);
            Assert.That(countAttribute.Order, Is.EqualTo(2));
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
                    It.IsAny<NuciApiRequestAuthorisationInfo>(),
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
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
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
        public void GivenAnUngErrorResponse_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionContainsTheUngFailureDetails()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync((NuciApiResponse)new NuciApiErrorResponse
                {
                    Code = "AUTHENTICATION_FAILURE",
                    Message = "The authentication has failed."
                });

            Assert.That(
                () => mobService.GetRandomMobName(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>()
                    .With.Message.Contains("AUTHENTICATION_FAILURE")
                    .And.Message.Contains("The authentication has failed."));
        }

        [Test]
        public void GivenAMissingBaseUrl_WhenGettingARandomMobName_ThenAnArgumentExceptionIsThrown()
        {
            settings.BaseUrl = string.Empty;

            Assert.That(
                () => mobService.GetRandomMobName(BuildGetMobNameRequest()),
                Throws.TypeOf<ArgumentException>());
        }

        private static GetMobNameRequest BuildGetMobNameRequest()
            => BuildGetMobNameRequest(MobType.WanderingTrader);

        private static GetMobNameRequest BuildGetMobNameRequest(MobType mobType) => new()
        {
            MobType = mobType
        };
    }
}