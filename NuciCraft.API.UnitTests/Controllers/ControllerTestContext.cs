using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NuciCraft.API.Configuration;

namespace NuciCraft.API.UnitTests.Controllers
{
    internal static class ControllerTestContext
    {
        private static string ApiKey => "NucileRullz!";

        internal static SecuritySettings BuildSecuritySettings() => new()
        {
            ApiKey = ApiKey
        };

        internal static void Initialise(ControllerBase controller)
        {
            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = $"Bearer {ApiKey}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}