using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail;
using SFA.DAS.EarlyConnect.Infrastructure.Extensions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenHandlingSendReminderEmailRequest
    {
        private Mock<ILogger<SendReminderEmailHandler>> _mockLogger;
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private SendReminderEmailHandler _sendReminderEmailHandler;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SendReminderEmailHandler>>();
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _sendReminderEmailHandler = new SendReminderEmailHandler(_mockLogger.Object, _mockOuterApiClient.Object);
        }

        [Test]
        public async Task Handle_SuccessfulReminderEmail_ReturnsLogId()
        {
            var reminderEmail = new ReminderEmail();
            var sendReminderEmailResponse = new SendReminderEmailResponse { Message = "Success" };
            _mockOuterApiClient.Setup(client => client.Post<SendReminderEmailResponse>(It.IsAny<SendReminderEmailRequest>(), true))
                              .ReturnsAsync(new ApiResponse<SendReminderEmailResponse>(sendReminderEmailResponse, HttpStatusCode.OK, ""));

            var message = await _sendReminderEmailHandler.Handle(reminderEmail);

            Assert.AreEqual(sendReminderEmailResponse.Message, message);
        }

        [Test]
        public void Handle_FailedReminderEmail_ThrowsException()
        {
            var reminderEmail = new ReminderEmail();
            var apiResponse = new ApiResponse<SendReminderEmailResponse>(null, HttpStatusCode.InternalServerError, "Simulated error message");
            _mockOuterApiClient.Setup(client => client.Post<SendReminderEmailResponse>(It.IsAny<SendReminderEmailRequest>(), true))
                              .ReturnsAsync(apiResponse);

            Assert.ThrowsAsync<ApiResponseException>(() => _sendReminderEmailHandler.Handle(reminderEmail));
        }
    }
}
