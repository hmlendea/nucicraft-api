using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuciCraft.API.IntegrationTests
{
    internal sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<string> requestedUrls = [];
        private Func<HttpRequestMessage, HttpResponseMessage> responder;

        internal StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            this.responder = responder;
        }

        internal IReadOnlyList<string> RequestedUrls
        {
            get
            {
                lock (requestedUrls)
                {
                    return [.. requestedUrls];
                }
            }
        }

        internal int RequestCount
        {
            get
            {
                lock (requestedUrls)
                {
                    return requestedUrls.Count;
                }
            }
        }

        internal void SetResponder(Func<HttpRequestMessage, HttpResponseMessage> value)
        {
            lock (requestedUrls)
            {
                responder = value;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Func<HttpRequestMessage, HttpResponseMessage> currentResponder;

            lock (requestedUrls)
            {
                requestedUrls.Add(GetUrl(request));
                currentResponder = responder;
            }

            return Task.FromResult(currentResponder(request));
        }

        internal static HttpResponseMessage JsonResponse(string json)
            => new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        internal static HttpResponseMessage FailingResponse(HttpStatusCode statusCode)
            => new(statusCode);

        internal static Func<HttpRequestMessage, HttpResponseMessage> Routing(
            Func<string, bool> isMatchingUrl,
            Func<HttpRequestMessage, HttpResponseMessage> matchingResponder,
            Func<HttpRequestMessage, HttpResponseMessage> fallbackResponder)
            => request =>
            {
                string url = GetUrl(request);

                if (isMatchingUrl(url))
                {
                    return matchingResponder(request);
                }

                return fallbackResponder(request);
            };

        internal static Func<HttpRequestMessage, HttpResponseMessage> Routing(
            Func<string, bool> isMatchingUrl,
            Func<string, bool> isSecondaryMatchingUrl,
            Func<HttpRequestMessage, HttpResponseMessage> matchingResponder,
            Func<HttpRequestMessage, HttpResponseMessage> secondaryMatchingResponder,
            Func<HttpRequestMessage, HttpResponseMessage> fallbackResponder)
            => request =>
            {
                string url = GetUrl(request);

                if (isMatchingUrl(url))
                {
                    return matchingResponder(request);
                }

                if (isSecondaryMatchingUrl(url))
                {
                    return secondaryMatchingResponder(request);
                }

                return fallbackResponder(request);
            };

        private static string GetUrl(HttpRequestMessage request)
        {
            Uri? requestUri = request.RequestUri;

            if (requestUri is null)
            {
                return string.Empty;
            }

            return requestUri.ToString();
        }
    }
}
