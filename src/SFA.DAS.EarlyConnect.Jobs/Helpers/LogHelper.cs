﻿using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Models.CreateLog;
using SFA.DAS.EarlyConnect.Models.UpdateLog;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.EarlyConnect.Jobs.Helpers
{
    public static class LogHelper
    {
        public static async Task<int> CreateLog(Stream fileStream, string fileName, string actionName, string requestResource, ICreateLogHandler _createLogHandler)
        {
            string fileContent;

            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memoryStream))
                {
                    fileContent = await reader.ReadToEndAsync();
                }
            }

            var createLog = new CreateLog
            {
                RequestType = actionName,
                RequestSource = requestResource,
                RequestIP = "",
                Payload = fileContent,
                FileName = fileName,
                Status = ImportStatus.InProgress.ToString()
            };

            var logId = await _createLogHandler.Handle(createLog);

            return logId;
        }

        public static async Task<int> CreateLog(string content, string fileName, string actionName, string requestResource, ICreateLogHandler _createLogHandler)
        {
            var createLog = new CreateLog
            {
                RequestType = actionName,
                RequestSource = requestResource,
                RequestIP = "",
                Payload = content,
                FileName = fileName,
                Status = ImportStatus.InProgress.ToString()
            };

            var logId = await _createLogHandler.Handle(createLog);

            return logId;
        }

        public static async Task UpdateLog(int logId, ImportStatus status, IUpdateLogHandler _updateLogHandler, string error = "")
        {
            var updateLog = new UpdateLog
            {
                LogId = logId,
                Status = status.ToString(),
                Error = error ?? string.Empty
            };

            await _updateLogHandler.Handle(updateLog);
        }
    }
}
