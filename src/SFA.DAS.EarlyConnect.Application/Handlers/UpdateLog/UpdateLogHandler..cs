using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;

namespace SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog
{
    public class UpdateLogHandler : IUpdateLogHandler
    {

        private readonly ILogger<UpdateLogHandler> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public UpdateLogHandler(
            ILogger<UpdateLogHandler> logger,
            IOuterApiClient outerApiClient
        )
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task Handle(Models.UpdateLog.UpdateLog updateLog)
        {
            _logger.LogInformation("about to handle update log");

            var response = await _outerApiClient.Post<object>(new UpdateLogRequest(updateLog), false);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("log updated");

        }
    }

}