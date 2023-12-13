using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.UpdateLog;

namespace SFA.DAS.EarlyConnect.Application.Handlers
{
    public interface IUpdateLogHandler
    {
        Task Handle(UpdateLog updateLog);
    }
}