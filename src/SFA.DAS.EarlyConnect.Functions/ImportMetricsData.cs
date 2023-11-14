using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.Handler;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class ImportMetricsData
    {
        private readonly IBlobService _blobService;
        private readonly IMetricsDataBulkUploadHandler _metricsDataBulkUploadHandler;
        private readonly string _container = "import-metricsdata";

        public ImportMetricsData(IMetricsDataBulkUploadHandler metricsDataBulkUploadHandler, IBlobService blobService, ILogger<ImportMetricsData> log)
        {
            this._metricsDataBulkUploadHandler = metricsDataBulkUploadHandler;
            _blobService = blobService;
        }
        [FunctionName("ImportMetricsData")]
        public async Task Run([BlobTrigger("import-metricsdata/{fileName}")] Stream fileStream, string fileName, ILogger log)
        {
            if (fileName.Contains("Metrics Data Upload"))
            {
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{fileName} \n Size: {fileStream.Length} Bytes");

                try
                {

                    await _metricsDataBulkUploadHandler.Handle(fileStream);

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
