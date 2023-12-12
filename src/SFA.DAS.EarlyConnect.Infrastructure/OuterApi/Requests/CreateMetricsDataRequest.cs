using SFA.DAS.EarlyConnect.Models.MetricsData;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class CreateMetricsDataRequest : IPostApiRequest
    {
        public object Data { get; set; }
        
        public CreateMetricsDataRequest(MetricsDataList metricsDataList)
        {
            Data = metricsDataList;
        }

        public string PostUrl => "metrics-data/add";
    }
}