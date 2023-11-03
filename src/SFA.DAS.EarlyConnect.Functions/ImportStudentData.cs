using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handler;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ImportStudentData
    {
        private readonly IBlobService _blobService;
        private readonly IStudentDataBulkUploadHandler _studentDataBulkUploadHandler;
        private readonly string _container = "studentdata";

        public ImportStudentData(IStudentDataBulkUploadHandler studentDataBulkUploadHandler, IBlobService blobService, ILogger<ImportStudentData> log)
        {
            this._studentDataBulkUploadHandler = studentDataBulkUploadHandler;
            _blobService = blobService;
        }
        [FunctionName("ImportStudentData")]
        public async Task Run([BlobTrigger("import-studentdata/{fileName}")] Stream fileStream, string fileName, ILogger log)
        {
            if (fileName.Contains("Student Data Upload"))
            {
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                try
                {

                    await _studentDataBulkUploadHandler.Handle(fileStream);

                    fileStream.Close();

                    //await _blobService.DeleteFile(fileName, _container);

                }
                catch (Exception ex)
                {
                    log.LogError($"Unable to import StudentData CSV: {ex}");
                    throw;
                }
            }

        }
    }
}
