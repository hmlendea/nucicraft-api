using System.Net.Http;
using System.Text;

namespace NuciCraft.API.IntegrationTests
{
    internal static class ApiRequestExtensions
    {
        internal static StringContent CreateJsonContent(this string content)
            => new(content, Encoding.UTF8, "application/json");
    }
}