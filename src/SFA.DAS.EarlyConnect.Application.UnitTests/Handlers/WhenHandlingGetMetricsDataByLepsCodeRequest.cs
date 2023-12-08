using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using System.Linq;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenHandlingGetMetricsDataByLepsCodeRequest
    {
        private Mock<ILogger<MetricsDataBulkExportHandler>> _loggerMock;
        private Mock<IOuterApiClient> _outerApiClientMock;
        private MetricsDataBulkExportHandler _metricsDataBulkExportHandler;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<MetricsDataBulkExportHandler>>();
            _outerApiClientMock = new Mock<IOuterApiClient>();
            _metricsDataBulkExportHandler =
                new MetricsDataBulkExportHandler(_loggerMock.Object, _outerApiClientMock.Object);
        }

        [Test]
        public async Task Handle_WithValidResponse_ReturnsBulkExportData()
        {
            string lepsCode = "123";
            var getMetricsDataByLepsCodeResponse = new GetMetricsDataByLepsCodeResponse
            {
                ListOfMetricsData = new List<ApprenticeshipMetricsData>
                {
                    new ApprenticeshipMetricsData
                    {
                        LogId = 1,
                        IntendedStartYear = 2022,
                        Region = "North",
                        MaxTravelInMiles = 50,
                        WillingnessToRelocate = true,
                        NoOfGCSCs = 5,
                        InterestAreas = new List<MetricsFlag>
                        {
                            new MetricsFlag { FlagCode = "A", FlagValue = true },
                            new MetricsFlag { FlagCode = "B", FlagValue = false }
                        },
                        NoOfStudents = 100
                    }
                }
            };

            _outerApiClientMock
                .Setup(client =>
                    client.Get<GetMetricsDataByLepsCodeResponse>(It.IsAny<GetMetricsDataByLepsCodeRequest>()))
                .ReturnsAsync(new ApiResponse<GetMetricsDataByLepsCodeResponse>(getMetricsDataByLepsCodeResponse,
                    HttpStatusCode.OK, ""));

            var result = await _metricsDataBulkExportHandler.Handle(lepsCode);

            Assert.NotNull(result);
            Assert.IsNotNull(result.ExportData);
            Assert.AreEqual(getMetricsDataByLepsCodeResponse.ListOfMetricsData.ToList()[0].LogId,
                result.LogId);

            Assert.IsNotEmpty(result.ExportData);
        }

        [Test]
        public void Handle_WithEmptyResponse_ThrowsInvalidOperationException()
        {
            string lepsCode = "123";
            var getMetricsDataByLepsCodeResponse = new GetMetricsDataByLepsCodeResponse
            { ListOfMetricsData = new List<ApprenticeshipMetricsData>() };

            _outerApiClientMock
                .Setup(client =>
                    client.Get<GetMetricsDataByLepsCodeResponse>(It.IsAny<GetMetricsDataByLepsCodeRequest>()))
                .ReturnsAsync(new ApiResponse<GetMetricsDataByLepsCodeResponse>(getMetricsDataByLepsCodeResponse,
                    HttpStatusCode.OK, ""));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _metricsDataBulkExportHandler.Handle(lepsCode));
        }

        [Test]
        public void Handle_WithNullResponse_ThrowsInvalidOperationException()
        {
            string lepsCode = "123";

            _outerApiClientMock
                .Setup(client =>
                    client.Get<GetMetricsDataByLepsCodeResponse>(It.IsAny<GetMetricsDataByLepsCodeRequest>()))
                .ReturnsAsync(new ApiResponse<GetMetricsDataByLepsCodeResponse>(null, HttpStatusCode.OK, ""));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _metricsDataBulkExportHandler.Handle(lepsCode));
        }
    }
}