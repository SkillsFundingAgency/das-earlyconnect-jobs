using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Models.CreateLog;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenHandlingCreateLogRequest
    {
        private Mock<ILogger<CreateLogHandler>> _mockLogger;
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private CreateLogHandler _createLogHandler;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CreateLogHandler>>();
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _createLogHandler = new CreateLogHandler(_mockLogger.Object, _mockOuterApiClient.Object);
        }

        [Test]
        public async Task Handle_SuccessfulLogCreation_ReturnsLogId()
        {
            var createLog = new CreateLog();
            var createLogResponse = new CreateLogResponse { LogId = 123 };
            _mockOuterApiClient.Setup(client => client.Post<CreateLogResponse>(It.IsAny<CreateLogRequest>(), true))
                              .ReturnsAsync(new ApiResponse<CreateLogResponse>(createLogResponse, HttpStatusCode.OK, ""));

            var logId = await _createLogHandler.Handle(createLog);

            Assert.AreEqual(createLogResponse.LogId, logId);
        }

        [Test]
        public void Handle_FailedLogCreation_ThrowsException()
        {
            var createLog = new CreateLog();
            var apiResponse = new ApiResponse<CreateLogResponse>(null, HttpStatusCode.InternalServerError, "Simulated error message");
            _mockOuterApiClient.Setup(client => client.Post<CreateLogResponse>(It.IsAny<CreateLogRequest>(), true))
                              .ReturnsAsync(apiResponse);

            Assert.ThrowsAsync<ApiResponseException>(() => _createLogHandler.Handle(createLog));
        }
    }
}
