
namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class GetMetricsDataByLepsCodeRequest : IGetApiRequest
    {
        public string LepsCode { get; set; }

        public GetMetricsDataByLepsCodeRequest(string lepsCode)
        {
            LepsCode = lepsCode;
        }

        public string GetUrl => $"metrics-data/{LepsCode}";
    }
}