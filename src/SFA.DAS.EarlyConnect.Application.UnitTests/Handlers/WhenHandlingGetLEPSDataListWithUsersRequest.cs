using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenHandlingGetLEPSDataListWithUsersRequest
    {
        private Mock<ILogger<GetLEPSDataWithUsersHandler>> _loggerMock;
        private Mock<IOuterApiClient> _outerApiClientMock;
        private GetLEPSDataWithUsersHandler _getLEPSDataWithUsersHandler;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<GetLEPSDataWithUsersHandler>>();
            _outerApiClientMock = new Mock<IOuterApiClient>();
            _getLEPSDataWithUsersHandler =
                new GetLEPSDataWithUsersHandler(_loggerMock.Object, _outerApiClientMock.Object);
        }

        [Test]
        public async Task Handle_WithValidResponse_ReturnsBulkExportData()
        {
            string lepsCode = "123";

            var lepsDataResult = new GetLEPSDataListWithUsersResponse
            {
                LEPSData = new List<GetLEPSDataWithUsersResponse>
                {
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 1 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 2 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 3 }
                }
            };

            _outerApiClientMock
                .Setup(client =>
                    client.Get<GetLEPSDataListWithUsersResponse>(It.IsAny<GetLEPSDataListWithUsersRequest>()))
                .ReturnsAsync(new ApiResponse<GetLEPSDataListWithUsersResponse>(lepsDataResult,
                    HttpStatusCode.OK, ""));

            var result = await _getLEPSDataWithUsersHandler.Handle();

            Assert.NotNull(result);
            Assert.IsNotNull(result.LEPSData);
            Assert.AreEqual(lepsDataResult, result);
        }
    }
}