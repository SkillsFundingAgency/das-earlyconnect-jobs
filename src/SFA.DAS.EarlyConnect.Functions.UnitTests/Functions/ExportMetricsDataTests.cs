using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers.BulkExport;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Functions.Configuration;
using SFA.DAS.EarlyConnect.Models.BulkExport;

namespace SFA.DAS.EarlyConnect.Functions.UnitTests.Functions
{
    [TestFixture]
    public class ExportMetricsDataTests
    {
        private Mock<IMetricsDataBulkExportHandler> mockMetricsDataBulkDownloadHandler;
        private Mock<IBlobService> mockBlobService;
        private Mock<IConfiguration> mockConfiguration;
        private ExportMetricsData exportMetricsData;
        private Mock<IOptions<FunctionConfiguration>> _configMock;

        [SetUp]
        public void Setup()
        {
            mockMetricsDataBulkDownloadHandler = new Mock<IMetricsDataBulkExportHandler>();
            mockBlobService = new Mock<IBlobService>();
            mockConfiguration = new Mock<IConfiguration>();
            _configMock = new Mock<IOptions<FunctionConfiguration>>();

            _configMock.Setup(x => x.Value).Returns(new FunctionConfiguration
            {
                ListOfRegions = "E37000025,E37000019,E37000051"
            });

            exportMetricsData = new ExportMetricsData(
                mockMetricsDataBulkDownloadHandler.Object,
                mockBlobService.Object,
                mockConfiguration.Object,
                _configMock.Object);
        }

        [Test]
        public async Task RunTimer_ShouldCallHandleAndUploadToBlob()
        {
            var responseMock = new Mock<Response<BlobContentInfo>>();
            mockConfiguration.Setup(config => config["ExportContainer"]).Returns("ExportContainer");

            mockMetricsDataBulkDownloadHandler
                 .Setup(handler => handler.Handle(It.IsAny<string>()))
                     .ReturnsAsync(new BulkExportData { ExportData = new List<List<KeyValuePair<string, string>>>(), FileName = "FileName" });

            mockBlobService
                 .Setup(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(responseMock.Object);

            await exportMetricsData.RunTimer(null, new Mock<ILogger>().Object);

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

            mockMetricsDataBulkDownloadHandler
              .Setup(handler => handler.Handle(It.IsAny<string>()))
                  .ReturnsAsync(new BulkExportData { ExportData = new List<List<KeyValuePair<string, string>>>(), FileName = "FileName" });

            mockBlobService
                  .Setup(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()))
                  .ReturnsAsync(responseMock.Object);

            var result = await exportMetricsData.RunHttp(mockHttpRequest.Object, new Mock<ILogger>().Object) as OkResult;

            Assert.IsNotNull(result);
            mockMetricsDataBulkDownloadHandler.Verify(handler => handler.Handle(It.IsAny<string>()), Times.Exactly(3));
            mockBlobService.Verify(blobService => blobService.UploadToBlob(It.IsAny<List<List<KeyValuePair<string, string>>>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
