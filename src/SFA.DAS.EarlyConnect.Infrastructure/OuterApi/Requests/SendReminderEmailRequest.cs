
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class SendReminderEmailRequest : IPostApiRequest
    {
        public object Data { get; set; }

        public SendReminderEmailRequest(ReminderEmail reminderEmail)
        {
            Data = reminderEmail;
        }
        public string PostUrl => "student-triage-data/reminder";
    }
}