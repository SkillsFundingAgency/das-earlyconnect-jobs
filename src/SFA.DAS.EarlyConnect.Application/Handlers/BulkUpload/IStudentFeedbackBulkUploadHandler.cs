using SFA.DAS.EarlyConnect.Models.BulkImport;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EarlyConnect.Application.Handlers.BulkUpload
{
    public interface IStudentFeedbackBulkUploadHandler
    {
        Task<BulkImportStatus> Handle(Stream fileStream, int logId);
    }
}
