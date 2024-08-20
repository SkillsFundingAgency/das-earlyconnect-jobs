using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.CreateLog;
using SFA.DAS.EarlyConnect.Application.Handlers.SendReminderEmail;
using SFA.DAS.EarlyConnect.Application.Handlers.UpdateLog;
using SFA.DAS.EarlyConnect.Functions;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Jobs.UnitTests.Functions
{
    [TestFixture]
    public class StudentSurveyReminderEmailTests
    {
        private Mock<ISendReminderEmailHandler> mockSendReminderEmailHandler;
        private Mock<ICreateLogHandler> _mockCreateLogHandler;
        private Mock<IUpdateLogHandler> _mockUpdateLogHandler;
        private StudentSurveyReminderEmail studentSurveyReminderEmail;
        private FunctionContext _functionContext;

        [SetUp]
        public void Setup()
        {
            mockSendReminderEmailHandler = new Mock<ISendReminderEmailHandler>();
            _mockCreateLogHandler = new Mock<ICreateLogHandler>();
            _mockUpdateLogHandler = new Mock<IUpdateLogHandler>();
            _functionContext = Mock.Of<FunctionContext>();

            studentSurveyReminderEmail = new StudentSurveyReminderEmail(
                mockSendReminderEmailHandler.Object,
                  _mockCreateLogHandler.Object,
                _mockUpdateLogHandler.Object);
        }

        //[Test]
        //public async Task RunTimer_ShouldCallHandleAndSendReminderEmail()
        //{
        //    mockSendReminderEmailHandler
        //         .Setup(handler => handler.Handle(It.IsAny<ReminderEmail>()))
        //             .ReturnsAsync("Success");

        //    await studentSurveyReminderEmail.RunTimer(null, new Mock<ILogger>().Object, _executionContext);

        //    mockSendReminderEmailHandler.Verify(handler => handler.Handle(It.IsAny<ReminderEmail>()), Times.Exactly(1));
        //}

        //[Test]
        //public async Task RunHttp_ShouldCallHandleAndSendReminderEmail()
        //{
        //    mockSendReminderEmailHandler
        //         .Setup(handler => handler.Handle(It.IsAny<ReminderEmail>()))
        //             .ReturnsAsync("Success");

        //    await studentSurveyReminderEmail.RunHttp(null, new Mock<ILogger>().Object, _executionContext);

        //    mockSendReminderEmailHandler.Verify(handler => handler.Handle(It.IsAny<ReminderEmail>()), Times.Exactly(1));
        //}
    }
}
