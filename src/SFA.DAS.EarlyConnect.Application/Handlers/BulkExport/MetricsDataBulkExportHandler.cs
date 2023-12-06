using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Models.BulkExport;

namespace SFA.DAS.EarlyConnect.Application.Handlers.BulkExport
{
    public class MetricsDataBulkExportHandler : IMetricsDataBulkExportHandler
    {

        private readonly ILogger<MetricsDataBulkExportHandler> _logger;
        private readonly IOuterApiClient _outerApiClient;


        public MetricsDataBulkExportHandler(
            ILogger<MetricsDataBulkExportHandler> logger,
            IOuterApiClient outerApiClient
        )
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<BulkExportData> Handle(string lepsCode)
        {
            _logger.LogInformation("About to handle metrics data export");

            int logId = 0;

            var response = await _outerApiClient.Get<GetMetricsDataByLepsCodeResponse>(new GetMetricsDataByLepsCodeRequest(lepsCode));

            if (response.Body?.ListOfMetricsData == null || !response.Body.ListOfMetricsData.Any())
            {
                _logger.LogInformation("No metrics data to export");
                throw new InvalidOperationException("No metrics data available for export");
            }

            var data = new List<List<KeyValuePair<string, string>>>();

            foreach (var item in response.Body.ListOfMetricsData)
            {
                if (item.InterestAreas == null)
                {
                    _logger.LogError("InterestAreas is null for an item");
                    throw new InvalidOperationException("InterestAreas is null for an item");
                }

                logId = item.LogId;

                var rowData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Intended_uni_entry_year", item.IntendedStartYear.ToString()),
                        new KeyValuePair<string, string>("Region", item.Region),
                        new KeyValuePair<string, string>("Max_travel_distance", item.MaxTravelInMiles.ToString()),
                        new KeyValuePair<string, string>("Willing_to_relocate_flag", item.WillingnessToRelocate.ToString()),
                        new KeyValuePair<string, string>("Number_gcse_grade4", item.NoOfGCSCs.ToString()),
                    };

                rowData.AddRange(item.InterestAreas.Select(interestArea =>
                    new KeyValuePair<string, string>(interestArea.FlagCode, interestArea.FlagValue.ToString())));

                rowData.Add(new KeyValuePair<string, string>("Students", item.NoOfStudents.ToString()));
                data.Add(rowData);
            }

            _logger.LogInformation("Data mapping completed for export");

            return new BulkExportData
            {
                ExportData = data,
                FileName = $"{lepsCode}_{logId}"
            };
        }
    }
}


