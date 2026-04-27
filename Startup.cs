using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuciAPI.Middleware;
using NuciAPI.Middleware.ExceptionHandling;
using NuciAPI.Middleware.Logging;
using NuciAPI.Middleware.Security;
using NuciCraft.API.Configuration;

namespace NuciCraft.API
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration => configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services
                .AddConfigurations(Configuration)
                .AddNuciApiScannerProtection()
                .AddCustomServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var dataStoreSettings = app.ApplicationServices.GetRequiredService<DataStoreSettings>();
            CreateStoreIfMissing(dataStoreSettings.RtpLocationsStorePath);
            CreateStoreIfMissing(dataStoreSettings.PlayersStorePath);

            app.UseNuciApiExceptionHandling();
            app.UseNuciApiScannerProtection();
            app.UseNuciApiRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static void CreateStoreIfMissing(string storePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storePath);

            var storeDirectory = Path.GetDirectoryName(storePath);

            if (!Directory.Exists(storeDirectory))
            {
                Directory.CreateDirectory(storeDirectory);
            }

            if (!File.Exists(storePath))
            {
                File.WriteAllText(storePath, "[]");
            }
        }
    }
}
