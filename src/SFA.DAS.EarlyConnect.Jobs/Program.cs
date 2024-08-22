using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Jobs.StartupExtensions;
using System;
using System.IO;
using System.Threading.Tasks;

//namespace Esfa.Recruit.Vacancies.Jobs
//{
//    class Program
//    {
//        public static async Task Main(string[] args)
//        {
//            ILogger logger = null;
//            try
//            {
//                var host = CreateHostBuilder().Build();
//                using (host)
//                {
//                    await host.RunAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                logger?.LogCritical(ex, "The Job has met with a horrible end!!");
//                throw;
//            }
//            finally
//            {
//                NLog.LogManager.Shutdown();
//            }
//        }

//        private static IHostBuilder CreateHostBuilder()
//        {
//            return
//                new HostBuilder()
//                    .ConfigureHostConfiguration(configHost =>
//                    {
//                        configHost.SetBasePath(Directory.GetCurrentDirectory());
//                        configHost.AddEnvironmentVariables();
//#if DEBUG
//                        configHost.AddJsonFile("local.settings.json", optional: true);
//#endif
//                    })
//                    .ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
//                    {

//                        configBuilder
//                            .AddAzureTableStorage(
//                                options =>
//                                {
//                                    options.ConfigurationKeys = hostBuilderContext.Configuration["ConfigNames"].Split(",");
//                                    options.EnvironmentName = hostBuilderContext.Configuration["Environment"];
//                                    options.StorageConnectionString = hostBuilderContext.Configuration["ConfigurationStorageConnectionString"];
//                                    options.PreFixConfigurationKeys = false;
//                                }
//                        );
//                    })
//                    .ConfigureServices((context, s) =>
//                    {
//                        var serviceProvider = s.BuildServiceProvider();
//                        var configuration = context.Configuration;

//                        var configBuilder = new ConfigurationBuilder()
//                        .AddConfiguration(configuration)
//                        .SetBasePath(Directory.GetCurrentDirectory())
//                        .AddEnvironmentVariables();

//                        var config = configBuilder.Build();

//                        s.Configure<OuterApiConfiguration>(config.GetSection("OuterApiConfiguration"));

//                        s.AddOptions();

//                        s.AddHttpClient<IOuterApiClient, OuterApiClient>();
//                        s.AddTransient<ICreateLogHandler, CreateLogHandler>();
//                        s.AddTransient<IMetricsDataBulkUploadHandler, MetricsDataBulkUploadHandler>();
//                        s.AddTransient<IStudentFeedbackBulkUploadHandler, StudentFeedbackBulkUploadHandler>();
//                        s.AddTransient<IMetricsDataBulkExportHandler, MetricsDataBulkExportHandler>();
//                        s.AddTransient<IGetLEPSDataWithUsersHandler, GetLEPSDataWithUsersHandler>();
//                        s.AddTransient<ICsvService, CsvService>();
//                        s.AddTransient<IUpdateLogHandler, UpdateLogHandler>();
//                        s.AddSingleton<IConfiguration>(config);
//                        s.AddTransient<IBlobService, BlobService>();
//                        s.AddTransient<IBlobContainerClientWrapper, BlobContainerClientWrapper>(x =>
//                            new BlobContainerClientWrapper(config.GetValue<string>("AzureWebJobsStorage")));

//                        s.AddApplicationInsightsTelemetryWorkerService(options =>
//                        {
//                            options.ConnectionString = config["APPINSIGHTS_INSTRUMENTATIONKEY"];
//                        });
//                        s.ConfigureFunctionsApplicationInsights();
//                    })
//                    .ConfigureLogging(logging =>
//                    {
//                        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
//                    });
//        }
//    }
// }


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((hostBuilderContext, builder) => { builder.BuildDasConfiguration(hostBuilderContext.Configuration); })
    .ConfigureServices((context, s) =>
    {
        var serviceProvider = s.BuildServiceProvider();
        var configuration = context.Configuration;

        var configBuilder = new ConfigurationBuilder()
        .AddConfiguration(configuration)
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables();

        var config = configBuilder.Build();

        s.Configure<OuterApiConfiguration>(config.GetSection("OuterApiConfiguration"));

        s.AddOptions();

        s.AddHttpClient<IOuterApiClient, OuterApiClient>();
        s.AddTransient<ICreateLogHandler, CreateLogHandler>();
        s.AddTransient<IMetricsDataBulkUploadHandler, MetricsDataBulkUploadHandler>();
        s.AddTransient<IStudentFeedbackBulkUploadHandler, StudentFeedbackBulkUploadHandler>();
        s.AddTransient<IMetricsDataBulkExportHandler, MetricsDataBulkExportHandler>();
        s.AddTransient<IGetLEPSDataWithUsersHandler, GetLEPSDataWithUsersHandler>();
        s.AddTransient<ICsvService, CsvService>();
        s.AddTransient<IUpdateLogHandler, UpdateLogHandler>();
        s.AddSingleton<IConfiguration>(config);
        s.AddTransient<IBlobService, BlobService>();
        s.AddTransient<IBlobContainerClientWrapper, BlobContainerClientWrapper>(x =>
            new BlobContainerClientWrapper(config.GetValue<string>("AzureWebJobsStorage")));

        s.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = config["APPINSIGHTS_INSTRUMENTATIONKEY"];
        });
        s.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
    })
    .Build();

host.Run();