using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.CreateLog;

namespace SFA.DAS.EarlyConnect.Application.Handlers
{
    public interface ICreateLogHandler
    {
        Task<int> Handle(CreateLog createLog);
    }
}
