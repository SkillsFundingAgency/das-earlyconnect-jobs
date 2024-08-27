using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Jobs.Helpers;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Functions
{
    public class StudentSurveyReminderEmail
    {
        private readonly ISendReminderEmailHandler _sendReminderEmailHandler;
        private readonly ICreateLogHandler _createLogHandler;
        private readonly IUpdateLogHandler _updateLogHandler;
        private readonly ILogger<StudentSurveyReminderEmail> _logger;

        public StudentSurveyReminderEmail(
            ISendReminderEmailHandler sendReminderEmailHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler,
            ILogger<StudentSurveyReminderEmail> logger)
        {
            _sendReminderEmailHandler = sendReminderEmailHandler;
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
            _logger = logger;
        }

        [Function("ReminderEmail_Timer")]
        public async Task RunTimer(
            [TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
        {
            await Run(null);
        }

        [Function("ReminderEmail_Http")]
        public async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await Run(req);
            return new OkResult();
        }

        private async Task Run(HttpRequest req)
        {
            int logId = 0;
            string region = null;
            try
            {
                ReminderEmail reminderEmail = new ReminderEmail { LepsCode = region };
                _logger.LogInformation($"Function triggered for reminder email {region}");

                logId = await LogHelper.CreateLog(JsonConvert.SerializeObject(reminderEmail), "", "ReminderEmail", "other", _createLogHandler);

                var sendReminderEmailResponse = await _sendReminderEmailHandler.Handle(reminderEmail);

                await LogHelper.UpdateLog(logId, ImportStatus.Completed, _updateLogHandler, sendReminderEmailResponse);

                _logger.LogInformation($"Function execution completed for reminder email {region}");
            }
            catch (Exception ex)
            {
                var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

                _logger.LogError($"Unable to send reminder email: {ex}");

                if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, $"Error posting reminder email {region}. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

                throw;
            }
        }
    }
}
