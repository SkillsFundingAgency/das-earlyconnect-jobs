using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Models.StudentData;
using SFA.DAS.EarlyConnect.Services.Interfaces.StudentDataService;
using System;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using System.Net;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Services.StudentDataService
{
    public class StudentDataService : IStudentDataService
    {
        private readonly ILogger<StudentDataService> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public StudentDataService(
            ILogger<StudentDataService> logger,
            IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<BulkImportStatus> CreateStudentData(StudentDataList studentDataList)
        {
            try
            {
                var response = await _outerApiClient.Post<CreateStudentDataResponse>(new CreateStudentDataRequest(studentDataList), false);
                return response.StatusCode == HttpStatusCode.OK
                    ? new BulkImportStatus { Status = ImportStatus.Completed }
                    : new BulkImportStatus { Status = ImportStatus.Failed };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create student data");
                throw;
            }
        }

    }
}
