using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ExportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkExportHandler _metricsDataBulkDownloadHandler;
        private readonly IGetLEPSDataWithUsersHandler _getLEPSDataWithUsersHandler;
        private readonly string _exportContainer;

        public ExportMetricsData(
            IMetricsDataBulkExportHandler metricsDataBulkDownloadHandler,
            IBlobService blobService,
            IConfiguration configuration,
            IGetLEPSDataWithUsersHandler getLEPSDataWithUsersHandler)
        {
            _metricsDataBulkDownloadHandler = metricsDataBulkDownloadHandler;
            _blobService = blobService;
            _exportContainer = configuration["ExportContainer"];
            _getLEPSDataWithUsersHandler = getLEPSDataWithUsersHandler;
        }

        [FunctionName("ExportMetricsData_Timer")]
        public async Task RunTimer(
            [TimerTrigger("%Functions:ExportMetricsDataJobSchedule%")] TimerInfo timerInfo,
            ILogger log)
        {
            await Run(null, log);
        }

        [FunctionName("ExportMetricsData_Http")]
        public async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            await Run(req, log);
            return new OkResult();
        }

        private async Task Run(HttpRequest req, ILogger log)
        {
            try
            {
                var lepsDataResult = await _getLEPSDataWithUsersHandler.Handle();

                if (lepsDataResult?.LEPSData == null || !lepsDataResult.LEPSData.Any())
                {
                    log.LogInformation("No LEPs data found");
                    return;
                }

                foreach (var lepsItem in lepsDataResult.LEPSData)
                {
                    log.LogInformation($"Function triggered for metrics data {lepsItem.LepCode}");

                    var metricsExportData = await _metricsDataBulkDownloadHandler.Handle(lepsItem.LepCode);

                    if (metricsExportData?.ExportData != null)
                    {
                        string blobName = $"{lepsItem.LepCode}_{metricsExportData.LogId}_{lepsItem.Id}";

                        await _blobService.UploadToBlob(metricsExportData.ExportData, _exportContainer, blobName);

                        log.LogInformation($"Function execution completed for metrics data {lepsItem.LepCode}");
                    }
                    else
                    {
                        log.LogInformation($"No data found for LEPs code {lepsItem.LepCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to import Metric Data CSV: {ex}");
                throw;
            }
        }

    }
}
