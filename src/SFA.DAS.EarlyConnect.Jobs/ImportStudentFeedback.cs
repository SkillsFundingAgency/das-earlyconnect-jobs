using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Functions;
using SFA.DAS.EarlyConnect.Jobs.Helpers;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EarlyConnect.Jobs
{
    public class ImportStudentFeedback
    {
        private readonly IBlobService _blobService;
        private readonly IStudentFeedbackBulkUploadHandler _studentFeedbackBulkUploadHandler;
        private readonly ICreateLogHandler _createLogHandler;
        private readonly IUpdateLogHandler _updateLogHandler;
        private readonly ILogger<ImportStudentFeedback> _logger;
        private readonly string _sourceContainer;
        private readonly string _archivedCompletedContainer;
        private readonly string _archivedFailedContainer;

        public ImportStudentFeedback(IStudentFeedbackBulkUploadHandler studentFeedbackBulkUploadHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler,
            IBlobService blobService,
            IConfiguration configuration,
            ILogger<ImportStudentFeedback> logger)
        {
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
            _studentFeedbackBulkUploadHandler = studentFeedbackBulkUploadHandler;
            _blobService = blobService;
            _sourceContainer = configuration["Containers:StudentFeedbackSourceContainer"];
            _archivedCompletedContainer = configuration["Containers:StudentFeedbackArchivedCompletedContainer"];
            _archivedFailedContainer = configuration["Containers:StudentFeedbackArchivedFailedContainer"];
            _logger = logger;
        }

        [Function("ImportStudentFeedback")]
        public async Task Run([BlobTrigger("import-studentfeedback/{fileName}")] Stream fileStream, string fileName, FunctionContext context)
        {
            int logId = 0;

            try
            {

                _logger.LogInformation($"Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                logId = await LogHelper.CreateLog(fileStream, fileName, context, "StudentFeedbackFile", _createLogHandler);

                _logger.LogInformation($"\n LOG ID:{logId} \n");

                var bulkImportStatus = await _studentFeedbackBulkUploadHandler.Handle(fileStream, logId);

                if (bulkImportStatus.Status == ImportStatus.Completed)
                {
                    _logger.LogInformation($"\n STATUS COMPLETED \n");
                    await LogHelper.UpdateLog(logId, ImportStatus.Completed, _updateLogHandler);
                    await _blobService.CopyBlobAsync(fileName, _sourceContainer, _archivedCompletedContainer);
                }
                else if (bulkImportStatus.Status == ImportStatus.Error)
                {
                    _logger.LogInformation($"\n STATUS ERROR \n");
                    await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, bulkImportStatus.Errors);
                    await _blobService.CopyBlobAsync(fileName, _sourceContainer, _archivedFailedContainer);
                }

                _logger.LogInformation($"Blob trigger function completed processing blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                fileStream.Close();
            }
            catch (Exception ex)
            {
                var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

                _logger.LogError($"Unable to import Student feedback CSV: {ex}");

                if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler,
                    $"Error posting student feedback. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

                throw;
            }
        }
    }
}
