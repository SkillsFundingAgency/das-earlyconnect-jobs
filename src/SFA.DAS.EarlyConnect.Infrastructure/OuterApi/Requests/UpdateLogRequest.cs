using SFA.DAS.EarlyConnect.Models.UpdateLog;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class UpdateLogRequest : IPostApiRequest
    {
        public object Data { get; set; }

        public UpdateLogRequest(UpdateLog updateLog)
        {
            Data = updateLog;
        }

        public string PostUrl => "api-log/update";
    }
}
