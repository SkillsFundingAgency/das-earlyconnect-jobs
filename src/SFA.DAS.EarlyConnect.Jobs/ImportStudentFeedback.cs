using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Application.Services;
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
        private readonly string _sourceContainer;
        private readonly string _archivedCompletedContainer;
        private readonly string _archivedFailedContainer;

        public ImportStudentFeedback(IStudentFeedbackBulkUploadHandler studentFeedbackBulkUploadHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler,
            IBlobService blobService,
            IConfiguration configuration)
        {
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
            _studentFeedbackBulkUploadHandler = studentFeedbackBulkUploadHandler;
            _blobService = blobService;

            _sourceContainer = configuration["Containers:SourceContainer"];
            _archivedCompletedContainer = configuration["Containers:ArchivedCompletedContainer"];
            _archivedFailedContainer = configuration["Containers:ArchivedFailedContainer"];
        }

        [FunctionName("ImportStudentFeedback")]
        public async Task Run([BlobTrigger("%Containers:SourceContainer%/{fileName}")] Stream fileStream, string fileName, ILogger log, ExecutionContext context)
        {
            int logId = 0;

            try
            {

                log.LogInformation($"Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                logId = await LogHelper.CreateLog(fileStream, fileName, context, "StudentFeedbackFile", _createLogHandler);

                var bulkImportStatus = await _studentFeedbackBulkUploadHandler.Handle(fileStream, logId);

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

                log.LogInformation($"Blob trigger function completed processing blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                fileStream.Close();
            }
            catch (Exception ex)
            {
                var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

                log.LogError($"Unable to import Student feedback CSV: {ex}");

                if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler,
                    $"Error posting student feedback. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

                throw;
            }
        }
    }
}
