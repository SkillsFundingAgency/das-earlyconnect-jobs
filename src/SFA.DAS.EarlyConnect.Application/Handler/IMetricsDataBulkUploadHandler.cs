using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Application.Handler
{
    public interface IMetricsDataBulkUploadHandler
    {
        Task<BulkImportStatus> Handle(Stream fileStream);
    }
}
