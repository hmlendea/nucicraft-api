using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

namespace NuciCraft.API.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class PersistenceApiTests
    {
        private static string StoreDirectoryName => "nucicraft-api-persistence-tests";

        [Test]
        public async Task GivenAWorld_WhenTheApplicationRestarts_ThenTheWorldPersists()
        {
            string storeDirectory = Path.Combine(
                Path.GetTempPath(),
                StoreDirectoryName,
                Guid.NewGuid().ToString("N"));

            try
            {
                using (ApiTestHost firstHost = new(storeDirectory))
                using (HttpClient firstClient = firstHost.CreateAuthorisedClient())
                {
                    HttpResponseMessage createResponse = await firstClient.PostAsync(
                        "/worlds",
                        "{\"id\":\"overworld\",\"hasWebMap\":true}".CreateJsonContent());

                    Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                }

                using ApiTestHost secondHost = new(storeDirectory);
                using HttpClient secondClient = secondHost.CreateAuthorisedClient();
                HttpResponseMessage getResponse = await secondClient.GetAsync("/worlds/overworld");

                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
            finally
            {
                if (Directory.Exists(storeDirectory))
                {
                    Directory.Delete(storeDirectory, true);
                }
            }
        }
    }
}