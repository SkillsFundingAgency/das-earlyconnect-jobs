using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;

namespace SFA.DAS.EarlyConnect.Jobs.StartupExtensions
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration BuildDasConfiguration(this IConfigurationBuilder configBuilder, IConfiguration configuration)
        {
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true);
#endif
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = ["SFA.DAS.EarlyConnect.Jobs"];
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            return configBuilder.Build();
        }
    }
}
