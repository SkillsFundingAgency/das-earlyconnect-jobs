using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Handlers.GetLEPSDataWithUsers;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Functions;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;
using SFA.DAS.EarlyConnect.Models.BulkExport;

namespace SFA.DAS.EarlyConnect.Jobs.UnitTests.Functions
{
    [TestFixture]
    public class ExportMetricsDataTests
    {
        private Mock<IMetricsDataBulkExportHandler> mockMetricsDataBulkDownloadHandler;
        private Mock<IGetLEPSDataWithUsersHandler> mockGetLEPSDataWithUsersHandler;
        private Mock<IBlobService> mockBlobService;
        private Mock<IConfiguration> mockConfiguration;
        private ExportMetricsData exportMetricsData;
        private ILogger<ExportMetricsData> _logger;

        [SetUp]
        public void Setup()
        {
            mockMetricsDataBulkDownloadHandler = new Mock<IMetricsDataBulkExportHandler>();
            mockGetLEPSDataWithUsersHandler = new Mock<IGetLEPSDataWithUsersHandler>();
            mockBlobService = new Mock<IBlobService>();
            mockConfiguration = new Mock<IConfiguration>();
            _logger = Mock.Of<ILogger<ExportMetricsData>>();

            exportMetricsData = new ExportMetricsData(
                mockMetricsDataBulkDownloadHandler.Object,
                mockBlobService.Object,
                mockConfiguration.Object,
                mockGetLEPSDataWithUsersHandler.Object,
                _logger);
        }

        [Test]
        public async Task RunTimer_ShouldCallHandleAndUploadToBlob()
        {
            var responseMock = new Mock<Response<BlobContentInfo>>();
            mockConfiguration.Setup(config => config["ExportContainer"]).Returns("ExportContainer");


            var lepsDataResult = new GetLEPSDataListWithUsersResponse
            {
                LEPSData = new List<GetLEPSDataWithUsersResponse>
                {
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 1 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 2 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 3 }
                }
            };

            mockGetLEPSDataWithUsersHandler.Setup(handler => handler.Handle()).ReturnsAsync(lepsDataResult);

            mockMetricsDataBulkDownloadHandler
                 .Setup(handler => handler.Handle(It.IsAny<string>()))
                     .ReturnsAsync(new BulkExportData { ExportData = new List<List<KeyValuePair<string, string>>>(), LogId = 1 });

            mockBlobService
                 .Setup(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(responseMock.Object);

            await exportMetricsData.RunTimer(null);

            mockMetricsDataBulkDownloadHandler.Verify(handler => handler.Handle(It.IsAny<string>()), Times.Exactly(3));
            mockBlobService.Verify(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Test]
        public async Task RunHttp_ShouldCallHandleAndUploadToBlob()
        {
            var responseMock = new Mock<Response<BlobContentInfo>>();
            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest.Setup(req => req.Headers[It.IsAny<string>()]).Returns(new Microsoft.Extensions.Primitives.StringValues("HeaderValue"));

            mockConfiguration.Setup(config => config["ExportContainer"]).Returns("ExportContainer");

            var lepsDataResult = new GetLEPSDataListWithUsersResponse
            {
                LEPSData = new List<GetLEPSDataWithUsersResponse>
                {
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 1 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 2 },
                    new GetLEPSDataWithUsersResponse { LepCode = "TestLepCode", Id = 3 }
                }
            };

            mockGetLEPSDataWithUsersHandler.Setup(handler => handler.Handle()).ReturnsAsync(lepsDataResult);

            mockMetricsDataBulkDownloadHandler
              .Setup(handler => handler.Handle(It.IsAny<string>()))
                  .ReturnsAsync(new BulkExportData { ExportData = new List<List<KeyValuePair<string, string>>>(), LogId = 1 });

            mockBlobService
                  .Setup(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()))
                  .ReturnsAsync(responseMock.Object);

            var result = await exportMetricsData.RunHttp(mockHttpRequest.Object) as OkResult;

            Assert.IsNotNull(result);
            mockMetricsDataBulkDownloadHandler.Verify(handler => handler.Handle(It.IsAny<string>()), Times.Exactly(3));
            mockBlobService.Verify(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
