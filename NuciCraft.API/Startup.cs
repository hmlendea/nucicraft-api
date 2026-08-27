using System;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NuciAPI.Middleware.ExceptionHandling;
using NuciAPI.Middleware.Logging;
using NuciAPI.Middleware.Security;

using NuciDAL.Repositories;

using NuciCraft.API.Configuration;
using NuciCraft.API.DataAccess.DataObjects;

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

        public void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment)
        {
            PrepareRepositories(applicationBuilder);

            applicationBuilder.UseNuciApiExceptionHandling();
            applicationBuilder.UseNuciApiScannerProtection();
            applicationBuilder.UseNuciApiRequestLogging();

            if (environment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseDefaultFiles();
            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseRouting();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void PrepareRepositories(IApplicationBuilder applicationBuilder)
        {
            DataStoreSettings dataStoreSettings = applicationBuilder.ApplicationServices.GetRequiredService<DataStoreSettings>();

            CreateStoreIfMissing(dataStoreSettings.RtpLocationsStorePath);
            CreateStoreIfMissing(dataStoreSettings.PlayersStorePath);
            CreateStoreIfMissing(dataStoreSettings.CountriesStorePath);
            CreateStoreIfMissing(dataStoreSettings.WorldsStorePath);
            CreateStoreIfMissing(dataStoreSettings.ZonesStorePath);

            EagerlyLoadRepositories(applicationBuilder.ApplicationServices);
        }

        private static void CreateStoreIfMissing(string storePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storePath);

            string storeDirectory = Path.GetDirectoryName(storePath);

            if (!Directory.Exists(storeDirectory))
            {
                Directory.CreateDirectory(storeDirectory);
            }

            if (!File.Exists(storePath))
            {
                File.WriteAllText(storePath, "[]");
            }
        }

        private static void EagerlyLoadRepositories(IServiceProvider serviceProvider)
        {
            EagerlyLoadRepository<PlayerDataObject>(serviceProvider);
            EagerlyLoadRepository<RtpLocationEntity>(serviceProvider);
            EagerlyLoadRepository<CountryDataObject>(serviceProvider);
            EagerlyLoadRepository<WorldDataObject>(serviceProvider);
            EagerlyLoadRepository<ZoneDataObject>(serviceProvider);
        }

        private static void EagerlyLoadRepository<TDataObject>(IServiceProvider serviceProvider)
            where TDataObject : NuciCraftEntityBase
            => serviceProvider
                .GetRequiredService<IFileRepository<TDataObject>>()
                .GetAll()
                .ToList();
    }
}
