using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Services.Interfaces.StudentDataService;
using SFA.DAS.EarlyConnect.Models.StudentData;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Application.Handler
{
    public class StudentDataBulkUploadHandler : IStudentDataBulkUploadHandler
    {

        private readonly ILogger<StudentDataBulkUploadHandler> _logger;
        private readonly IStudentDataService _studentDataService;
        private readonly ICsvService _csvService;


        public StudentDataBulkUploadHandler(
            ILogger<StudentDataBulkUploadHandler> logger,
            IStudentDataService studentDataService,
            ICsvService csvService
        )
        {
            _csvService = csvService;
            _logger = logger;
            _studentDataService = studentDataService;
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

                try
                {
                    var listOfStudentData = await _csvService.ParseCsvData(sr);

                    var studentDataList = new StudentDataList { ListOfStudentData = listOfStudentData };

                    return await _studentDataService.CreateStudentData(studentDataList);
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


