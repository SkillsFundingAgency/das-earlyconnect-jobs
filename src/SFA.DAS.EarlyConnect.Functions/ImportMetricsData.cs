using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Models.CreateLog;
using SFA.DAS.EarlyConnect.Models.UpdateLog;


namespace SFA.DAS.EarlyConnect.Functions
{
    public class ImportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkUploadHandler _metricsDataBulkUploadHandler;
        private readonly ICreateLogHandler _createLogHandler;
        private readonly IUpdateLogHandler _updateLogHandler;
        private readonly string _container = "import-metricsdata";

        public ImportMetricsData(IMetricsDataBulkUploadHandler metricsDataBulkUploadHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler,
            IBlobService blobService)
        {
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
            _metricsDataBulkUploadHandler = metricsDataBulkUploadHandler;
            _blobService = blobService;
        }
        [FunctionName("ImportMetricsData")]
        public async Task Run([BlobTrigger("import-metricsdata/{fileName}")] Stream fileStream, string fileName, ILogger log, ExecutionContext context)
        {
            int logId = 0;

            try
            {
                log.LogInformation($"Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                logId = await CreateLog(ImportStatus.InProgress, fileStream, fileName, context);

                var bulkImportStatus = await _metricsDataBulkUploadHandler.Handle(fileStream);

                if (bulkImportStatus.Status == ImportStatus.Completed)
                {
                    await UpdateLog(logId, ImportStatus.Completed);
                }
                else if (bulkImportStatus.Status == ImportStatus.Error)
                {
                    await UpdateLog(logId, ImportStatus.Error, bulkImportStatus.Errors);
                }

                fileStream.Close();

            }
            catch (Exception ex)
            {
                log.LogError($"Unable to import Metric Data CSV: {ex}");
                if (logId > 0) await UpdateLog(logId, ImportStatus.Error, $"{ex.Message} - {ex.StackTrace}");
                throw;
            }

        }
        private async Task<int> CreateLog(ImportStatus status, Stream fileStream, string fileName, ExecutionContext context)
        {
            string fileContent;
            var actionName = context.FunctionName;

            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memoryStream))
                {
                    fileContent = await reader.ReadToEndAsync();
                }
            }

            var createLog = new CreateLog
            {
                RequestType = actionName,
                RequestSource = "UCAS",
                RequestIP = "",
                Payload = fileContent,
                FileName = fileName,
                Status = ImportStatus.InProgress.ToString()
            };

            var logId = await _createLogHandler.Handle(createLog);

            return logId;
        }

        private async Task UpdateLog(int logId, ImportStatus status, string error = "")
        {
            var updateLog = new UpdateLog
            {
                LogId = logId,
                Status = status.ToString(),
                Error = error ?? string.Empty
            };

            await _updateLogHandler.Handle(updateLog);
        }
    }
}
