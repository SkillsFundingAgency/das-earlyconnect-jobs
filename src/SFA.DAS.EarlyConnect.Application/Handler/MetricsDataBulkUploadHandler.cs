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
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using System.Net;
using SFA.DAS.EarlyConnect.Models.MetricsData;

namespace SFA.DAS.EarlyConnect.Application.Handler
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
            _logger.LogInformation("about to handle student data import");

            using (var sr = new StreamReader(fileStream))
            {
                //var fileStatus = await ValidateImportStream(sr);

                //if (!fileStatus.ImportFileIsValid)
                //{
                //    return fileStatus;
                //}

                //IList<dynamic> contacts = null;

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
                            Region = contactDictionary.TryGetValue("Region", out var regionValue) ? regionValue?.ToString() : string.Empty,
                            IntendedStartYear = decimal.TryParse(contactDictionary.TryGetValue("Intended_uni_entry_year", out var intendedStartYearValue) ? intendedStartYearValue?.ToString() : "0", out var intendedStartYear) ? intendedStartYear : 0,
                            MaxTravelInMiles = int.TryParse(contactDictionary.TryGetValue("Max_travel_distance", out var maxTravelInMilesValue) ? maxTravelInMilesValue?.ToString() : "0", out var maxTravelInMiles) ? maxTravelInMiles : 0,
                            WillingnessToRelocate = bool.TryParse(contactDictionary.TryGetValue("willing_to_relocate_flag", out var willingnessToRelocateValue) ? willingnessToRelocateValue?.ToString() : "false", out var relocationParsedValue) && relocationParsedValue,
                            NoOfGCSCs = int.TryParse(contactDictionary.TryGetValue("Number_gcse_grade4", out var noOfGCSCsValue) ? noOfGCSCsValue?.ToString() : "0", out var gcseParsedValue) ? gcseParsedValue : 0,
                            NoOfStudents = int.TryParse(contactDictionary.TryGetValue("Students", out var noOfStudentsValue) ? noOfStudentsValue?.ToString() : "0", out var studentsParsedValue) ? studentsParsedValue : 0,
                            LogId = 1,
                            MetricFlags = new List<string>()
                        };

                        foreach (var kvp in contactDictionary)
                        {
                            string valueAsString = kvp.Value?.ToString()?.Trim()?.ToUpperInvariant();
                            string keyAsString = kvp.Key?.ToString()?.Trim()?.ToUpperInvariant();

                            if (!IsSpecifiedField(keyAsString) && IsSpecifiedValue(valueAsString))
                            {
                                metricData.MetricFlags.Add(kvp.Key);

                            }
                        }

                        filledObjects.Add(metricData);
                    }

                    var metricsDataList = new MetricsDataList { MetricsData = filledObjects.ToList() };

                    var response = await _outerApiClient.Post<CreateMetricsDataResponse>(new CreateMetricsDataRequest(metricsDataList), false);
                    return response.StatusCode == HttpStatusCode.OK
                        ? new BulkImportStatus { Status = ImportStatus.Completed }
                        : new BulkImportStatus { Status = ImportStatus.Failed };
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to process CSV file");

                    var status = new BulkImportStatus
                    {
                        ImportFileIsValid = false,
                        ValidationError = "Unable to parse CSV file, the format of the file is invalid"
                    };

                    return status;
                }
            }
        }

        bool IsSpecifiedField(string key) =>
            key.ToUpperInvariant() == "REGION" || key.ToUpperInvariant() == "INTENDED_UNI_ENTRY_YEAR" ||
            key.ToUpperInvariant() == "MAX_TRAVEL_DISTANCE" || key.ToUpperInvariant() == "WILLING_TO_RELOCATE_FLAG" ||
            key.ToUpperInvariant() == "NUMBER_GCSE_GRADE4" || key.ToUpperInvariant() == "STUDENTS";


        bool IsSpecifiedValue(string value)
        {
            return value == "1" || value == "Y" || value == "YES";
        }

        private async Task<BulkImportStatus> ValidateImportStream(StreamReader sr)
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
    }
}


