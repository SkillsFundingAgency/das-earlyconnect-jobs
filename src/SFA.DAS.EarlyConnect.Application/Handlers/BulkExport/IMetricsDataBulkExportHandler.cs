using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.BulkExport;

namespace SFA.DAS.EarlyConnect.Application.Handlers.BulkExport
{
    public interface IMetricsDataBulkExportHandler
    {
        Task<BulkExportData> Handle(string lepsCode);
    }
}
