using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

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
    public sealed class MobServiceTests
    {
        private static int MaximumVillageSchemaAttempts => 8192;

        private static int VillageSchemaVariantsCount => 2;

        private static string GetRandomVillageSchemaMethodName => "GetRandomVillageSchema";

        private static string GetSchemaForMobTypeMethodName => "GetSchemaForMobType";

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
        public async Task GivenASupportedMobType_WhenGettingRandomMobNames_ThenTheGeneratedNamesAreReturned()
        {
            IEnumerable<string> expectedNames = ["Ilarion"];
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync(BuildGenerateNamesApiResponse(expectedNames));

            IEnumerable<string> generatedNames = await mobService.GetRandomMobNameAsync(
                BuildGetMobNameRequest());

            Assert.That(generatedNames, Is.EqualTo(expectedNames));
        }

        [Test]
        public async Task GivenASupportedMobType_WhenGettingARandomMobName_ThenTheUniversalNameGeneratorRequestIsBuiltCorrectly()
        {
            GenerateNamesRequest capturedRequest = null;
            NuciApiRequestAuthorisationInfo capturedAuthorisationInfo = null;
            GetMobNameRequest request = BuildGetMobNameRequest();
            request.Count = 4;

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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Ilarion"]));

            await mobService.GetRandomMobNameAsync(request);

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedAuthorisationInfo, Is.Not.Null);
            Assert.That(
                capturedAuthorisationInfo.BearerToken,
                Is.EqualTo(settings.ApiKey));
            Assert.That(
                capturedRequest.Schema,
                Is.EqualTo("romanian-persons-male"));
            Assert.That(capturedRequest.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task GivenTheEnderDragonMobType_WhenGettingARandomMobName_ThenTheFantasyDragonsSchemaIsUsed()
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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Smaug"]));

            await mobService.GetRandomMobNameAsync(
                BuildGetMobNameRequest(MobType.EnderDragon));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Schema, Is.EqualTo("fantasy-dragons"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GivenTheCowMobType_WhenGettingARandomMobName_ThenTheRomanianAnimalsCowsSchemaIsUsed()
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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Ilarion"]));

            await mobService.GetRandomMobNameAsync(
                BuildGetMobNameRequest(MobType.Cow));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Schema, Is.EqualTo("romanian-animals-cows"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GivenThePigMobType_WhenGettingARandomMobName_ThenTheRomanianAnimalsPigsSchemaIsUsed()
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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Ilarion"]));

            await mobService.GetRandomMobNameAsync(
                BuildGetMobNameRequest(MobType.Pig));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Schema, Is.EqualTo("animals-pigs"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [TestCase("evoker")]
        [TestCase("illusioner")]
        [TestCase("pillager")]
        [TestCase("vindicator")]
        public async Task GivenTheNewIllagerMobType_WhenGettingARandomMobName_ThenThePinchedUniverseZaganianPersonsMaleSchemaIsUsed(
            string mobTypeName)
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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Radu"]));

            await mobService.GetRandomMobNameAsync(new GetMobNameRequest
            {
                MobType = mobTypeName
            });

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(
                capturedRequest.Schema,
                Is.EqualTo("pinched-zaganian-persons-male"));
            Assert.That(capturedRequest.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GivenTheVillageMobType_WhenGettingARandomMobName_ThenARomanianPersonSchemaIsUsed()
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
                .ReturnsAsync(BuildGenerateNamesApiResponse(["Mary"]));

            await mobService.GetRandomMobNameAsync(
                BuildGetMobNameRequest(MobType.Villager));

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
        public async Task GivenAnUnsupportedMobType_WhenGettingARandomMobName_ThenANotImplementedExceptionIsThrown()
        {
            GetMobNameRequest request = BuildGetMobNameRequest();
            request.MobType = "zombie";

            Assert.That(
                () => mobService.GetRandomMobNameAsync(request),
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
        public async Task GivenAnEmptyUniversalNameGeneratorResponse_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionIsThrown()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync(BuildGenerateNamesApiResponse([]));

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task GivenNullNamesInTheUniversalNameGeneratorResponse_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionIsThrown()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync(BuildGenerateNamesApiResponse(null));

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task GivenASuccessfulUnexpectedResponseType_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionIsThrown()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync((NuciApiResponse)new NuciApiSuccessResponse());

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task GivenANullUniversalNameGeneratorResponse_WhenGettingARandomMobName_ThenAnArgumentNullExceptionIsThrown()
        {
            universalNameGeneratorClientMock
                .Setup(client => client
                    .SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>(
                        HttpMethod.Get,
                        It.IsAny<GenerateNamesRequest>(),
                        It.IsAny<NuciApiRequestAuthorisationInfo>(),
                        "Names"))
                .ReturnsAsync((NuciApiResponse)null);

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public async Task GivenAnUngErrorResponse_WhenGettingARandomMobName_ThenAnInvalidOperationExceptionContainsTheUngFailureDetails()
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
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<InvalidOperationException>()
                    .With.Message.Contains("AUTHENTICATION_FAILURE")
                    .And.Message.Contains("The authentication has failed."));
        }

        [Test]
        public async Task GivenAMissingBaseUrl_WhenGettingARandomMobName_ThenAnArgumentExceptionIsThrown()
        {
            settings.BaseUrl = string.Empty;

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public async Task GivenAMissingApiKey_WhenGettingARandomMobName_ThenAnArgumentExceptionIsThrown()
        {
            settings.ApiKey = string.Empty;

            Assert.That(
                () => mobService.GetRandomMobNameAsync(BuildGetMobNameRequest()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public async Task GivenANullRequest_WhenGettingARandomMobName_ThenAnArgumentNullExceptionIsThrown()
            => Assert.That(
                () => mobService.GetRandomMobNameAsync(null),
                Throws.TypeOf<ArgumentNullException>());

        [Test]
        public async Task GivenAWhitespaceMobType_WhenGettingARandomMobName_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => mobService.GetRandomMobNameAsync(new GetMobNameRequest { MobType = " " }),
                Throws.TypeOf<ArgumentException>());

        [Test]
        public void GivenAnUnsupportedMobType_WhenGettingItsSchema_ThenANotImplementedExceptionIsThrown()
            => Assert.That(
                () => InvokeGetSchemaForMobType(MobType.Unsupported),
                Throws.TypeOf<NotImplementedException>());

        [Test]
        public void GivenRepeatedVillageSchemaSelections_WhenSelectingSchemas_ThenBothVariantsAreReturned()
        {
            HashSet<string> schemas = [];

            for (int attemptCount = 0;
                attemptCount < MaximumVillageSchemaAttempts && schemas.Count < VillageSchemaVariantsCount;
                attemptCount += 1)
            {
                schemas.Add(InvokeGetRandomVillageSchema());
            }

            Assert.That(
                schemas,
                Is.EquivalentTo([
                    "romanian-persons-male",
                    "romanian-persons-female",
                ]));
        }

        private static GenerateNamesResponse BuildGenerateNamesApiResponse(
            IEnumerable<string> names)
            => new()
            {
                Names = names
            };

        private static GetMobNameRequest BuildGetMobNameRequest()
            => BuildGetMobNameRequest(MobType.WanderingTrader);

        private static GetMobNameRequest BuildGetMobNameRequest(MobType mobType) => new()
        {
            MobType = mobType
        };

        private static string InvokeGetRandomVillageSchema()
        {
            MethodInfo method = typeof(MobService).GetMethod(
                GetRandomVillageSchemaMethodName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Func<string> getRandomVillageSchema = method.CreateDelegate<Func<string>>();

            return getRandomVillageSchema();
        }

        private static string InvokeGetSchemaForMobType(MobType mobType)
        {
            MethodInfo method = typeof(MobService).GetMethod(
                GetSchemaForMobTypeMethodName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Func<MobType, string> getSchemaForMobType = method.CreateDelegate<Func<MobType, string>>();

            return getSchemaForMobType(mobType);
        }
    }
}