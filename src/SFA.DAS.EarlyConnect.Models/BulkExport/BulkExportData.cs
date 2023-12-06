using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Models.BulkExport
{
    public class BulkExportData
    {
        public string FileName { get; set; }
        public List<List<KeyValuePair<string, string>>> ExportData { get; set; }
    }
}

