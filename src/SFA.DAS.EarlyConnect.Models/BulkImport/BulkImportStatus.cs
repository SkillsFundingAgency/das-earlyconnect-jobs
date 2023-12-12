using System;
using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Models.BulkImport
{
    public class BulkImportStatus
    {
        public BulkImportStatus()
        {
            StartTime = DateTime.Now;
        }
        public string Container { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public double Duration => (DateTime.Now - StartTime).TotalMilliseconds;
        public ImportStatus Status;
        public bool ImportFileIsValid { get; set; } = true;
        public string ValidationError { get; set; }
        public IEnumerable<string> HeaderErrors { get; set; } = new List<string>();
        public string ApiErrors { get; set; }
    }
}