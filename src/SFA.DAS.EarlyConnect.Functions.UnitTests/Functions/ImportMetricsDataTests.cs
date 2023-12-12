using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Functions.UnitTests.Functions
{
    [TestFixture]
    public class ImportMetricsDataTests
    {
        private Mock<IBlobService> _blobServiceMock;
        private Mock<IMetricsDataBulkUploadHandler> _metricsDataBulkUploadHandlerMock;
        private Mock<ILogger<ImportMetricsData>> _loggerMock;
        private ImportMetricsData _importMetricsData;

        [SetUp]
        public void Setup()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _metricsDataBulkUploadHandlerMock = new Mock<IMetricsDataBulkUploadHandler>();
            _loggerMock = new Mock<ILogger<ImportMetricsData>>();

            _importMetricsData = new ImportMetricsData(
                _metricsDataBulkUploadHandlerMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task Run_WithValidFileName_CallsHandleMethod()
        {
            var fileName = "Metrics Data Upload_123.csv";
            var stream = new MemoryStream();

            await _importMetricsData.Run(stream, fileName, _loggerMock.Object);

            _metricsDataBulkUploadHandlerMock.Verify(
                 x => x.Handle(It.IsAny<Stream>()),
                 Times.Once
             );
        }
    }
}
