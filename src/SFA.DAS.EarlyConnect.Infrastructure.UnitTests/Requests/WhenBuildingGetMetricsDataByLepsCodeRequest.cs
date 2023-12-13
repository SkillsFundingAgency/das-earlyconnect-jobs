using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.CreateLog;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.Requests
{

    [TestFixture]
    public class WhenBuildingGetMetricsDataByLepsCodeRequest
    {
        [Test, AutoData]
        public void Then_The_Url_WhenBuildingGetMetricsDataByLepsCodeRequestIs_Correctly_Constructed(CreateLog createLog)
        {
            string LepsCode = "123";

            var actual = new GetMetricsDataByLepsCodeRequest(LepsCode);

            actual.GetUrl.Should().Be($"metrics-data/{LepsCode}");
        }

    }
}