using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.CreateLog;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.Requests
{

    [TestFixture]
    public class WhenBuildingCreateLogRequest
    {
        [Test, AutoData]
        public void Then_The_Url_WhenBuildingCreateLogRequestIs_Correctly_Constructed(CreateLog createLog)
        {
            var actual = new CreateLogRequest(createLog);

            actual.PostUrl.Should().Be("api-log/add");
        }

    }
}