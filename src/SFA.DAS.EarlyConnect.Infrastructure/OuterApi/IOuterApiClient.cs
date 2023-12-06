using System.Threading.Tasks;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi
{
    public interface IOuterApiClient
    {
        Task<ApiResponse<TResponse>> Post<TResponse>(IPostApiRequest request, bool includeResponse = true);
        Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request);
    }
}