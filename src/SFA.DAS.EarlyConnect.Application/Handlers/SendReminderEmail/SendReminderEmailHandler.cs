using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail
{
    public class SendReminderEmailHandler : ISendReminderEmailHandler
    {
        private readonly ILogger<SendReminderEmailHandler> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public SendReminderEmailHandler(
            ILogger<SendReminderEmailHandler> logger,
            IOuterApiClient outerApiClient
        )
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<string> Handle(ReminderEmail reminderEmail)
        {
            _logger.LogInformation("about to handle reminder email");

            var response = await _outerApiClient.Post<SendReminderEmailResponse>(new SendReminderEmailRequest(reminderEmail), true);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("reminder email");

            return response.Body.Message;

        }
    }
}



