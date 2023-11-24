using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Functions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.EarlyConnect.Functions
{
    public class Startup : FunctionsStartup
    {
        public Startup() { }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true);
#endif

            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigName"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });


            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
            ConfigureServices(builder.Services, config);

        }

        public void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<OuterApiConfiguration>(configuration.GetSection("OuterApiConfiguration"));

            services.AddOptions();

            services.AddHttpClient<IOuterApiClient, OuterApiClient>();
            services.AddTransient<ICreateLogHandler, CreateLogHandler>();
            services.AddTransient<IMetricsDataBulkUploadHandler, MetricsDataBulkUploadHandler>();
            services.AddTransient<IUpdateLogHandler, UpdateLogHandler>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IBlobService, BlobService>(x =>
                new BlobService(configuration.GetValue<string>("AzureWebJobsStorage")));

            var executioncontextoptions = services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            services.AddLogging((options) =>
            {
                options.SetMinimumLevel(LogLevel.Trace);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });

            });

            services.AddApplicationInsightsTelemetry(configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }
    }
}
