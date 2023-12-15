using System.Threading.Tasks;

namespace SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog
{
    public interface IUpdateLogHandler
    {
        Task Handle(Models.UpdateLog.UpdateLog updateLog);
    }
}