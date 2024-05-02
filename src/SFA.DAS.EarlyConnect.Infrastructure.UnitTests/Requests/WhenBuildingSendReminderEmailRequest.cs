using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.SendReminderEmail;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.Requests
{

    [TestFixture]
    public class WhenBuildingSendReminderEmailRequest
    {
        [Test, AutoData]
        public void Then_The_Url_WhenBuildingCreateLogRequestIs_Correctly_Constructed(ReminderEmail reminderEmail)
        {
            var actual = new SendReminderEmailRequest(reminderEmail);

            actual.PostUrl.Should().Be("student-triage-data/reminder");
        }

    }
}