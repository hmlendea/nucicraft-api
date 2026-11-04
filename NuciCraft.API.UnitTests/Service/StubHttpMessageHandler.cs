using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuciCraft.API.UnitTests.Service
{
    public sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responder =
            responder ?? throw new ArgumentNullException(nameof(responder));

        public HttpRequestMessage? LastRequest { get; private set; }

        public Task<HttpResponseMessage> SendAsyncPublic(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => SendAsync(request, cancellationToken);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(responder(request));
        }

        public static StubHttpMessageHandler ReturningJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new(_ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        public static StubHttpMessageHandler FailingWith(HttpStatusCode statusCode)
            => new(_ => new HttpResponseMessage(statusCode));
    }
}
