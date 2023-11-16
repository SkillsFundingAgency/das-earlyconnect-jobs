using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Application.Services;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using System.Net;
using SFA.DAS.EarlyConnect.Models.MetricsData;

namespace SFA.DAS.EarlyConnect.Application.Handlers
{
    public class MetricsDataBulkUploadHandler : IMetricsDataBulkUploadHandler
    {

        private readonly ILogger<MetricsDataBulkUploadHandler> _logger;
        private readonly ICsvService _csvService;
        private readonly IOuterApiClient _outerApiClient;


        public MetricsDataBulkUploadHandler(
            ILogger<MetricsDataBulkUploadHandler> logger,
            IOuterApiClient outerApiClient,
            ICsvService csvService
        )
        {
            _csvService = csvService;
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<BulkImportStatus> Handle(Stream fileStream)
        {
            _logger.LogInformation("about to handle metrics data import");

            using (var sr = new StreamReader(fileStream))
            {
                var fileStatus = ValidateImportStream(sr);

                if (!fileStatus.ImportFileIsValid)
                {
                    return fileStatus;
                }


                try
                {
                    IList<dynamic> contacts = null;
                    contacts = await _csvService.ConvertToList(sr);

                    List<MetricsData> filledObjects = new List<MetricsData>();

                    foreach (var contact in contacts)
                    {
                        IDictionary<string, object> contactDictionary = contact;

                        var metricData = new MetricsData
                        {
                            Region = ExtractText(contactDictionary.TryGetValue("Region", out var regionValue) ? regionValue?.ToString() : string.Empty),
                            IntendedStartYear = ParseDecimal(contactDictionary, "Intended_uni_entry_year"),
                            MaxTravelInMiles = CalculateMiles(contactDictionary.TryGetValue("Max_travel_distance", out var maxTravelInMilesValue) ? maxTravelInMilesValue?.ToString()?.Trim() : "0"),
                            WillingnessToRelocate = ParseBoolean(contactDictionary, "Willing_to_relocate_flag"),
                            NoOfGCSCs = ParseInteger(contactDictionary, "Number_gcse_grade4"),
                            NoOfStudents = ParseInteger(contactDictionary, "Students"),
                            LogId = 1,
                            MetricFlags = new List<string>()
                        };

                        foreach (var kvp in contactDictionary)
                        {
                            string valueAsString = kvp.Value?.ToString()?.Trim()?.ToUpperInvariant();
                            string keyAsString = kvp.Key?.Trim().ToUpperInvariant();

                            if (!IsSpecifiedField(keyAsString) && IsSpecifiedValue(valueAsString))
                            {
                                metricData.MetricFlags.Add(kvp.Key);

                            }
                        }

                        filledObjects.Add(metricData);
                    }

                    var metricsDataList = new MetricsDataList { MetricsData = filledObjects.ToList() };

                    var response = await _outerApiClient.Post<object>(new CreateMetricsDataRequest(metricsDataList), false);

                    return response.StatusCode == HttpStatusCode.OK
                        ? new BulkImportStatus { Status = ImportStatus.Completed, ApiErrors = response.ErrorContent }
                        : new BulkImportStatus { Status = ImportStatus.Error, ApiErrors = response.ErrorContent };
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to process CSV file");

                    var bulkImportStatus = new BulkImportStatus
                    {
                        Status = ImportStatus.Error,
                        ValidationError = "Unable to parse CSV file, the format of the file is invalid"
                    };

                    return bulkImportStatus;
                }
            }
        }

        private BulkImportStatus ValidateImportStream(StreamReader sr)
        {

            var status = new BulkImportStatus();

            if (_csvService.IsEmpty(sr))
            {
                status.ValidationError = "No headers - File is empty so cannot be processed";
            }

            if (_csvService.HasData(sr) == false)
            {
                status.ValidationError = "Missing data - there is no data to process";

            }
            return status;
        }

        decimal ParseDecimal(IDictionary<string, object> dict, string key) =>
            decimal.TryParse(dict.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : "0", out var result) ? result : 0;

        int ParseInteger(IDictionary<string, object> dict, string key) =>
            int.TryParse(dict.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : "0", out var result) ? result : 0;

        bool ParseBoolean(IDictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                var trimmedValue = value?.ToString()?.Trim()?.ToLowerInvariant();
                return IsSpecifiedValue(trimmedValue);
            }

            return false;
        }

        bool IsSpecifiedField(string key) =>
            key == "REGION" || key == "INTENDED_UNI_ENTRY_YEAR" ||
            key == "MAX_TRAVEL_DISTANCE" || key == "WILLING_TO_RELOCATE_FLAG" ||
            key == "NUMBER_GCSE_GRADE4" || key == "STUDENTS";

        bool IsSpecifiedValue(string value) =>
            value == "1" || value == "Y" || value == "YES";

        static int CalculateMiles(string text)
        {
            string[] parts = text.Split('_');

            if (parts.Length == 3 && int.TryParse(parts[0], out int lowerBound) && int.TryParse(parts[1], out int upperBound))
            {
                return (lowerBound + upperBound) / 2;
            }

            return 0;
        }

        static string ExtractText(string input)
        {
            return input.Trim('!', ' ');
        }

    }
}


