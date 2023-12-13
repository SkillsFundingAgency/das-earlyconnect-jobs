using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses
{
    public class GetLEPSDataListWithUsersResponse
    {
        public ICollection<GetLEPSDataWithUsersResponse> LEPSData { get; set; }
    }
}