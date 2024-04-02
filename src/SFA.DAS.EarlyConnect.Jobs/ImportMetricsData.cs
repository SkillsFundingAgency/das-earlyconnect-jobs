using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Jobs;
using SFA.DAS.EarlyConnect.Jobs.Helpers;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ImportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkUploadHandler _metricsDataBulkUploadHandler;
        private readonly ICreateLogHandler _createLogHandler;
        private readonly IUpdateLogHandler _updateLogHandler;
        private readonly ILogger<ImportMetricsData> _logger;
        private readonly string _sourceContainer;
        private readonly string _archivedCompletedContainer;
        private readonly string _archivedFailedContainer;

        public ImportMetricsData(IMetricsDataBulkUploadHandler metricsDataBulkUploadHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler,
            IBlobService blobService,
            IConfiguration configuration,
            ILogger<ImportMetricsData> logger)
        {
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
            _metricsDataBulkUploadHandler = metricsDataBulkUploadHandler;
            _blobService = blobService;

            _sourceContainer = configuration["Containers:MetricsDataSourceContainer"];
            _archivedCompletedContainer = configuration["Containers:MetricsDataArchivedCompletedContainer"];
            _archivedFailedContainer = configuration["Containers:MetricsDataArchivedFailedContainer"];
            _logger = logger;
        }

        [Function("ImportMetricsData")]
        public async Task Run([BlobTrigger("import-metricsdata/{fileName}")] Stream fileStream, string fileName, FunctionContext context)
        {
            int logId = 0;

            try
            {

                _logger.LogInformation($"Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                logId = await LogHelper.CreateLog(fileStream, fileName, context, "UCAS", _createLogHandler);

                var bulkImportStatus = await _metricsDataBulkUploadHandler.Handle(fileStream, logId);

                if (bulkImportStatus.Status == ImportStatus.Completed)
                {
                    await LogHelper.UpdateLog(logId, ImportStatus.Completed, _updateLogHandler);
                    await _blobService.CopyBlobAsync(fileName, _sourceContainer, _archivedCompletedContainer);

                }
                else if (bulkImportStatus.Status == ImportStatus.Error)
                {
                    await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, bulkImportStatus.Errors);
                    await _blobService.CopyBlobAsync(fileName, _sourceContainer, _archivedFailedContainer);
                }

                _logger.LogInformation($"Blob trigger function completed processing blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                fileStream.Close();
            }
            catch (Exception ex)
            {
                var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

                _logger.LogError($"Unable to import Metric Data CSV: {ex}");

                if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, $"Error posting Metrics data. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

                throw;
            }
        }
    }
}
