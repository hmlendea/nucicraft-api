using NUnit.Framework;

using NuciCraft.API.Responses;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetResponseTests
    {
        [Test]
        public void GivenContent_WhenBuildingAResponse_ThenTheContentIsRetained()
        {
            object content = "Nucile rullz";

            GetResponse response = new(content);

            Assert.That(response.Content, Is.SameAs(content));
        }
    }
}