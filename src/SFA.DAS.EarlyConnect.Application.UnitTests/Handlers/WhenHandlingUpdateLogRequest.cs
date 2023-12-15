using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.UpdateLog;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenHandlingUpdateLogRequest
    {
        private Mock<ILogger<UpdateLogHandler>> _mockLogger;
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private UpdateLogHandler _updateLogHandler;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<UpdateLogHandler>>();
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _updateLogHandler = new UpdateLogHandler(_mockLogger.Object, _mockOuterApiClient.Object);
        }

        [Test]
        public void Handle_SuccessfulLogUpdate_NoExceptionThrown()
        {
            var updateLog = new UpdateLog();
            var apiResponse = new ApiResponse<object>(null, HttpStatusCode.OK, "");
            _mockOuterApiClient.Setup(client => client.Post<object>(It.IsAny<UpdateLogRequest>(), false))
                .ReturnsAsync(apiResponse);

            async Task Act() => await _updateLogHandler.Handle(updateLog);

            Assert.DoesNotThrowAsync(Act);
        }

        [Test]
        public void Handle_FailedLogUpdate_ThrowsException()
        {
            var updateLog = new UpdateLog();
            var apiResponse = new ApiResponse<object>(null, HttpStatusCode.InternalServerError, "Simulated error message");
            _mockOuterApiClient.Setup(client => client.Post<object>(It.IsAny<UpdateLogRequest>(), false))
                .ReturnsAsync(apiResponse);

            Assert.ThrowsAsync<ApiResponseException>(() => _updateLogHandler.Handle(updateLog));
        }
    }
}
