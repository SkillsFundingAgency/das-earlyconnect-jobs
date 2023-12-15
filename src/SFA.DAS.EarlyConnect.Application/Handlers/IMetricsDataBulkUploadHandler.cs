﻿using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Application.Handlers
{
    public interface IMetricsDataBulkUploadHandler
    {
        Task<BulkImportStatus> Handle(Stream fileStream,int logId);
    }
}
