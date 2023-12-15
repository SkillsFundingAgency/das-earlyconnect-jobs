using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;

namespace SFA.DAS.EarlyConnect.Application.Handlers.CreateLog
{
    public class CreateLogHandler : ICreateLogHandler
    {

        private readonly ILogger<CreateLogHandler> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public CreateLogHandler(
            ILogger<CreateLogHandler> logger,
            IOuterApiClient outerApiClient
        )
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<int> Handle(Models.CreateLog.CreateLog createLog)
        {
            _logger.LogInformation("about to handle create log");

            var response = await _outerApiClient.Post<CreateLogResponse>(new CreateLogRequest(createLog), true);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("log created");

            return response.Body.LogId;

        }
    }
}



