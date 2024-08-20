using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail
{
    public interface ISendReminderEmailHandler
    {
        Task<string> Handle(ReminderEmail reminderEmail);
    }
}
