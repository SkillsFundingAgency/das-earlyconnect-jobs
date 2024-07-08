using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Application.Services;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using System.Net;
using SFA.DAS.EarlyConnect.Models.StudentFeedback;
using SFA.DAS.EarlyConnect.Application.Helpers;
using Azure;

namespace SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload
{
    public class StudentFeedbackBulkUploadHandler : IStudentFeedbackBulkUploadHandler
    {
        private readonly ILogger<StudentFeedbackBulkUploadHandler> _logger;
        private readonly ICsvService _csvService;
        private readonly IOuterApiClient _outerApiClient;

        public StudentFeedbackBulkUploadHandler(
            ILogger<StudentFeedbackBulkUploadHandler> logger,
            IOuterApiClient outerApiClient,
            ICsvService csvService
        )
        {
            _csvService = csvService;
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<BulkImportStatus> Handle(Stream fileStream, int logId)
        {
            _logger.LogInformation("Handling student feedback data import");

            using (var sr = new StreamReader(fileStream))
            {
                var fileStatus = ValidateImportStream(sr);

                if (fileStatus.Status == ImportStatus.Error)
                {
                    return fileStatus;
                }

                try
                {
                    IList<dynamic> contacts = null;
                    contacts = await _csvService.ConvertToList(sr);


                    List<StudentFeedback> filledObjects = new List<StudentFeedback>();

                    foreach (var contact in contacts)
                    {
                        IDictionary<string, object> contactDictionary = contact;

                        var studentFeedback = new StudentFeedback
                        {
                            SurveyId = TextHelper.ExtractGuid(contactDictionary.TryGetValue("SurveyId", out var surveyIdValue) ? surveyIdValue?.ToString() : string.Empty),
                            StatusUpdate = TextHelper.ExtractText(contactDictionary.TryGetValue("StatusUpdate", out var statusUpdateValue) ? statusUpdateValue?.ToString() : string.Empty),
                            Notes = TextHelper.ExtractText(contactDictionary.TryGetValue("Notes", out var notesValue) ? notesValue?.ToString() : string.Empty),
                            UpdatedBy = TextHelper.ExtractText(contactDictionary.TryGetValue("UpdatedBy", out var updatedByValue) ? updatedByValue?.ToString() : string.Empty),
                            LogId = logId
                        };

                        filledObjects.Add(studentFeedback);
                    }

                    var studentFeedbackList = new StudentFeedbackList { ListOfStudentFeedback = filledObjects.ToList() };

                    var response = await _outerApiClient.Post<object>(new CreateStudentFeedbackRequest(studentFeedbackList), false);

                    _logger.LogInformation($"\n STATUS CODE FOR OUTER API RESPONSE: {response.StatusCode} \n");

                    return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created
                        ? new BulkImportStatus { Status = ImportStatus.Completed, Errors = response.ErrorContent }
                        : new BulkImportStatus { Status = ImportStatus.Error, Errors = response.ErrorContent };
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to process CSV file");

                    var bulkImportStatus = new BulkImportStatus
                    {
                        Status = ImportStatus.Error,
                        Errors = "Unable to parse CSV file, the format of the file is invalid"
                    };

                    return bulkImportStatus;
                }
            }
        }

        private BulkImportStatus ValidateImportStream(StreamReader sr)
        {
            var importStatus = new BulkImportStatus();

            if (_csvService.IsEmpty(sr))
            {
                importStatus.Errors = "No headers - File is empty so cannot be processed";
            }

            else if (!_csvService.HasData(sr))
            {
                importStatus.Errors = "Missing data - there is no data to process";
            }

            else if (!HasMandatoryData(sr))
            {
                importStatus.Errors = "One or more required fields are missing in the CSV header";
            }

            if (importStatus.Errors != null)
            {
                importStatus.Status = ImportStatus.Error;
            }

            return importStatus;
        }

        public static bool HasMandatoryData(StreamReader stream, ILogger<StudentFeedbackBulkUploadHandler> logger)
        {
            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, SeekOrigin.Begin);

            var headerLine = stream.ReadLine();

            logger.LogInformation("Header line: ", headerLine);

            if (headerLine != null)
            {
                var headers = headerLine.Split(',');

                logger.LogInformation("Headers: ", headers);

                return headers.Contains("SurveyId") &&
                       headers.Contains("StatusUpdate") &&
                       headers.Contains("Notes") &&
                       headers.Contains("UpdatedBy");
            }

            return false;
        }
    }
}
