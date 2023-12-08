using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;

namespace SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers
{
    public interface IGetLEPSDataWithUsersHandler
    {
        Task<GetLEPSDataListWithUsersResponse> Handle();
    }
}
