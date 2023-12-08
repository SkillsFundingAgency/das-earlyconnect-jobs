using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;

namespace SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers
{
    public class GetLEPSDataWithUsersHandler : IGetLEPSDataWithUsersHandler
    {
        private readonly ILogger<GetLEPSDataWithUsersHandler> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public GetLEPSDataWithUsersHandler(
            ILogger<GetLEPSDataWithUsersHandler> logger,
            IOuterApiClient outerApiClient
        )
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<GetLEPSDataListWithUsersResponse> Handle()
        {
            _logger.LogInformation("About to handle get LEPS data with users");

            var response = await _outerApiClient.Get<GetLEPSDataListWithUsersResponse>(new GetLEPSDataListWithUsersRequest());

            _logger.LogInformation("About to handle get LEPS data with users completed");

            return response.Body;
        }
    }
}


