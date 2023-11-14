using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Models.MetricsData
{
    public class MetricsDataList
    {
        public IEnumerable<MetricsData> MetricsData { get; set; }
    }
    public class MetricsData
    {
        public string Region { get; set; }
        public decimal IntendedStartYear { get; set; }
        public int MaxTravelInMiles { get; set; }
        public bool WillingnessToRelocate { get; set; }
        public int NoOfGCSCs { get; set; }
        public int NoOfStudents { get; set; }
        public int LogId { get; set; }
        public IList<string> MetricFlags { get; set; }
    }
}
