using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NuciCraft.API.UnitTests.Service
{
    public sealed class RoutingHandler(
        StubHttpMessageHandler geolocationHandler,
        StubHttpMessageHandler anonymisationHandler) : HttpMessageHandler
    {
        private readonly StubHttpMessageHandler geolocationHandler = geolocationHandler;
        private readonly StubHttpMessageHandler anonymisationHandler = anonymisationHandler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string? url = request.RequestUri?.ToString();

            if (url is not null && (url.Contains("funkemunky.cc", StringComparison.Ordinal) || url.Contains("proxycheck.io", StringComparison.Ordinal)))
            {
                return anonymisationHandler.SendAsyncPublic(request, cancellationToken);
            }

            return geolocationHandler.SendAsyncPublic(request, cancellationToken);
        }
    }
}
