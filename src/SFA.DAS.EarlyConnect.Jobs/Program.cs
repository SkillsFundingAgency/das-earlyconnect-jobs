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
using SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Jobs.StartupExtensions;
using System.IO;

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
        s.AddTransient<ISendReminderEmailHandler, SendReminderEmailHandler>();

        s.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = config["APPINSIGHTS_INSTRUMENTATIONKEY"];
        });
        s.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("Microsoft", LogLevel.Warning);
    })
    .Build();

host.Run();