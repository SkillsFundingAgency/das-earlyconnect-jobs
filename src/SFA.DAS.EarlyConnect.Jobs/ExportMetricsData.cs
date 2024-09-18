using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;
using SFA.DAS.EarlyConnect.Application.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ExportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkExportHandler _metricsDataBulkDownloadHandler;
        private readonly IGetLEPSDataWithUsersHandler _getLEPSDataWithUsersHandler;
        private readonly ILogger<ExportMetricsData> _logger;
        private readonly string _exportContainer;

        public ExportMetricsData(
            IMetricsDataBulkExportHandler metricsDataBulkDownloadHandler,
            IBlobService blobService,
            IConfiguration configuration,
            IGetLEPSDataWithUsersHandler getLEPSDataWithUsersHandler,
            ILogger<ExportMetricsData> logger)
        {
            _metricsDataBulkDownloadHandler = metricsDataBulkDownloadHandler;
            _blobService = blobService;
            _exportContainer = configuration["Containers:MetricsDataExportContainer"];
            _getLEPSDataWithUsersHandler = getLEPSDataWithUsersHandler;
            _logger = logger;
        }

        [Function("ExportMetricsData_Timer")]
        public async Task RunTimer(
            [TimerTrigger("*/2 * * * *")] TimerInfo timerInfo)
        {
            await Run(null);
        }

        [Function("ExportMetricsData_Http")]
        public async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await Run(req);
            return new OkResult();
        }

        private async Task Run(HttpRequest req)
        {
            try
            {
                var lepsDataResult = await _getLEPSDataWithUsersHandler.Handle();

                if (lepsDataResult?.LEPSData == null || !lepsDataResult.LEPSData.Any())
                {
                    _logger.LogInformation("No LEPs data found");
                    return;
                }

                foreach (var lepsItem in lepsDataResult.LEPSData)
                {
                    _logger.LogInformation($"Function triggered for metrics data {lepsItem.LepCode}");

                    var metricsExportData = await _metricsDataBulkDownloadHandler.Handle(lepsItem.LepCode);

                    if (metricsExportData?.ExportData != null)
                    {
                        string blobName = $"{lepsItem.LepCode}_{metricsExportData.LogId}_{lepsItem.Id}";

                        await _blobService.UploadToBlob(metricsExportData.ExportData, _exportContainer, blobName);

                        _logger.LogInformation($"Function execution completed for metrics data {lepsItem.LepCode}");
                    }
                    else
                    {
                        _logger.LogInformation($"No data found for LEPs code {lepsItem.LepCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to import Metric Data CSV: {ex}");
                throw;
            }
        }

    }
}
