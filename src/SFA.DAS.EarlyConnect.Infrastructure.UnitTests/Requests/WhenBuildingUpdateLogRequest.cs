using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.UpdateLog;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.Requests
{

    [TestFixture]
    public class WhenBuildingUpdateLogRequest
    {
        [Test, AutoData]
        public void Then_The_Url_WhenBuildingUpdateLogRequestIs_Correctly_Constructed(UpdateLog updateLog)
        {
            var actual = new UpdateLogRequest(updateLog);

            actual.PostUrl.Should().Be("api-log/update");
        }

    }
}