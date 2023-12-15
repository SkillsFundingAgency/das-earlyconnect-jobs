using SFA.DAS.EarlyConnect.Models.CreateLog;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class CreateLogRequest : IPostApiRequest
    {
        public object Data { get; set; }

        public CreateLogRequest(CreateLog createLog)
        {
            Data = createLog;
        }

        public string PostUrl => "api-log/add";
    }
}
