using SFA.DAS.EarlyConnect.Models.StudentData;
using System.Threading.Tasks;
using System.Net;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Services.Interfaces.StudentDataService
{
    public interface IStudentDataService
    {
        Task<BulkImportStatus> CreateStudentData(StudentDataList studentDataList);
    }
}
