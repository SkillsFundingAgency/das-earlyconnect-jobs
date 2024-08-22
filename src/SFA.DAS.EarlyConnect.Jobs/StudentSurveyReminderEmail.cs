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

        public StudentSurveyReminderEmail(
            ISendReminderEmailHandler sendReminderEmailHandler,
            ICreateLogHandler createLogHandler,
            IUpdateLogHandler updateLogHandler)
        {
            _sendReminderEmailHandler = sendReminderEmailHandler;
            _createLogHandler = createLogHandler;
            _updateLogHandler = updateLogHandler;
        }

        [Function("ReminderEmail_Timer")]
        public async Task RunTimer(
            [TimerTrigger("0 0 * * *")] TimerInfo timerInfo,
            ILogger log)
        {
            await Run(null, log);
        }

        [Function("ReminderEmail_Http")]
        public async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            await Run(req, log);
            return new OkResult();
        }

        private async Task Run(HttpRequest req, ILogger log)
        {
            int logId = 0;
            string region = null;
            try
            {
                ReminderEmail reminderEmail = new ReminderEmail { LepsCode = region };
                log.LogInformation($"Function triggered for reminder email {region}");

                logId = await LogHelper.CreateLog(JsonConvert.SerializeObject(reminderEmail), "", "ReminderEmail", "other", _createLogHandler);

                var sendReminderEmailResponse = await _sendReminderEmailHandler.Handle(reminderEmail);

                await LogHelper.UpdateLog(logId, ImportStatus.Completed, _updateLogHandler, sendReminderEmailResponse);

                log.LogInformation($"Function execution completed for reminder email {region}");
            }
            catch (Exception ex)
            {
                var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

                log.LogError($"Unable to send reminder email: {ex}");

                if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, $"Error posting reminder email {region}. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

                throw;
            }
        }

        //private async Task Run(HttpRequest req, ILogger log, ExecutionContext context)
        //{
        //    int logId = 0;
        //    string region = LepsRegion.NorthEast;
        //    try
        //    {
        //        ReminderEmail reminderEmail = new ReminderEmail { LepsCode = region };
        //        log.LogInformation($"Function triggered for reminder email {region}");

        //        logId = await LogHelper.CreateLog(JsonConvert.SerializeObject(reminderEmail), "", context, "other", _createLogHandler);

        //        var sendReminderEmailResponse = await _sendReminderEmailHandler.Handle(reminderEmail);

        //        await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, sendReminderEmailResponse);

        //        log.LogInformation($"Function execution completed for reminder email {region}");
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorMessage = (ex as Infrastructure.Extensions.ApiResponseException)?.Error;

        //        log.LogError($"Unable to send reminder email: {ex}");

        //        if (logId > 0) await LogHelper.UpdateLog(logId, ImportStatus.Error, _updateLogHandler, $"Error posting reminder email {region}. {(errorMessage != null ? $"\nErrorInfo: {errorMessage}" : "")}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}");

        //        throw;
        //    }
        //}
    }
}
