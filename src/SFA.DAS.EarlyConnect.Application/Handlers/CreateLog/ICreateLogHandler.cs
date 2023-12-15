using System.Threading.Tasks;

namespace SFA.DAS.EarlyConnect.Application.Handlers.CreateLog
{
    public interface ICreateLogHandler
    {
        Task<int> Handle(Models.CreateLog.CreateLog createLog);
    }
}
