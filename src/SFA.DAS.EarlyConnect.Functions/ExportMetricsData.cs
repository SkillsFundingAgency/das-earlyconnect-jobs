using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Functions.Extensions;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ExportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkExportHandler _metricsDataBulkDownloadHandler;
        private readonly string _exportContainer;

        public ExportMetricsData(
            IMetricsDataBulkExportHandler metricsDataBulkDownloadHandler,
            IBlobService blobService,
            IConfiguration configuration)
        {
            _metricsDataBulkDownloadHandler = metricsDataBulkDownloadHandler;
            _blobService = blobService;
            _exportContainer = configuration["ExportContainer"];
        }

        [FunctionName("ExportMetricsData_Timer")]
        public async Task RunTimer(
            [TimerTrigger(Schedules.MidnightDaily, RunOnStartup = true)] TimerInfo timerInfo,
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
            string lepsCode = "E37000025,E37000019,E37000051";

            try
            {
                foreach (var item in lepsCode.Split(','))
                {
                    log.LogInformation("Function triggered.");

                    var bulkExportStatus = await _metricsDataBulkDownloadHandler.Handle(item);

                    if (bulkExportStatus.ExportData != null)
                    {
                        await _blobService.UploadToBlob(bulkExportStatus.ExportData, _exportContainer, bulkExportStatus.FileName);
                    }
                    else
                    {
                        log.LogInformation($"No data found for the LEPS code {item}");
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
