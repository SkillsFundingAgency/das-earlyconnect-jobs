using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using System;
using System.IO;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(s =>
    {
        // serviceProvider
        var serviceProvider = s.BuildServiceProvider();
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
            options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
            options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            options.EnvironmentName = configuration["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        });


        var config = configBuilder.Build();
        s.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
        // ConfigureServices(builder.Services, config);

        s.Configure<OuterApiConfiguration>(configuration.GetSection("OuterApiConfiguration"));

        s.AddOptions();

        s.AddHttpClient<IOuterApiClient, OuterApiClient>();
        s.AddTransient<ICreateLogHandler, CreateLogHandler>();
        s.AddTransient<IMetricsDataBulkUploadHandler, MetricsDataBulkUploadHandler>();
        s.AddTransient<IStudentFeedbackBulkUploadHandler, StudentFeedbackBulkUploadHandler>();
        s.AddTransient<IMetricsDataBulkExportHandler, MetricsDataBulkExportHandler>();
        s.AddTransient<IGetLEPSDataWithUsersHandler, GetLEPSDataWithUsersHandler>();
        s.AddTransient<ICsvService, CsvService>();
        s.AddTransient<IUpdateLogHandler, UpdateLogHandler>();
        s.AddSingleton<IConfiguration>(configuration);
        s.AddTransient<IBlobService, BlobService>();
        s.AddTransient<IBlobContainerClientWrapper, BlobContainerClientWrapper>(x =>
            new BlobContainerClientWrapper(configuration.GetValue<string>("AzureWebJobsStorage")));

        // logging and app insights
        s.AddLogging((options) =>
        {
            options.SetMinimumLevel(LogLevel.Trace);
            options.SetMinimumLevel(LogLevel.Trace);
            options.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });

        });
        s.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
        });
        s.ConfigureFunctionsApplicationInsights();

    })
    .Build();

host.Run();