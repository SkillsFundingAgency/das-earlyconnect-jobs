using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.MetricsData;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.Requests
{

    [TestFixture]
    public class WhenBuildingCreateMetricsDataRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed(MetricsDataList metricsDataList)
        {
            var actual = new CreateMetricsDataRequest(metricsDataList);

            actual.PostUrl.Should().Be("metrics-data/add");
        }

    }
}