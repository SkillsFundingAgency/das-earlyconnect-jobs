using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Models.BulkImport;
using SFA.DAS.EarlyConnect.Models.UpdateLog;

namespace SFA.DAS.EarlyConnect.Functions.UnitTests.Functions
{
    [TestFixture]
    public class ImportMetricsDataTests
    {
        private Mock<IMetricsDataBulkUploadHandler> _mockMetricsDataBulkUploadHandler;
        private Mock<ICreateLogHandler> _mockCreateLogHandler;
        private Mock<IUpdateLogHandler> _mockUpdateLogHandler;
        private Mock<IBlobService> _mockBlobService;

        private ImportMetricsData _importMetricsData;
        private string _fileName;
        private Stream _fileStream;
        private ILogger _logger;
        private ExecutionContext _executionContext;

        [SetUp]
        public void SetUp()
        {
            _mockMetricsDataBulkUploadHandler = new Mock<IMetricsDataBulkUploadHandler>();
            _mockCreateLogHandler = new Mock<ICreateLogHandler>();
            _mockUpdateLogHandler = new Mock<IUpdateLogHandler>();
            _mockBlobService = new Mock<IBlobService>();

            _importMetricsData = new ImportMetricsData(
                _mockMetricsDataBulkUploadHandler.Object,
                _mockCreateLogHandler.Object,
                _mockUpdateLogHandler.Object,
                _mockBlobService.Object);

            _fileName = "testFile.csv";
            _fileStream = new MemoryStream();
            _logger = Mock.Of<ILogger>();
            _executionContext = Mock.Of<ExecutionContext>();
        }

        [Test]
        public async Task Run_SuccessCase_UpdatesLogForCompletedStatus()
        {
            _mockMetricsDataBulkUploadHandler
                .Setup(h => h.Handle(It.IsAny<Stream>()))
                .ReturnsAsync(new BulkImportStatus { Status = ImportStatus.Completed });

            await _importMetricsData.Run(_fileStream, _fileName, _logger, _executionContext);

            _mockUpdateLogHandler.Verify(
                x => x.Handle(It.Is<UpdateLog>(ul => ul.Status == ImportStatus.Completed.ToString())),
                Times.Once
            );
        }

        [Test]
        public async Task Run_FailedCase_UpdatesLogForErrorStatus()
        {
            _mockMetricsDataBulkUploadHandler
                .Setup(h => h.Handle(It.IsAny<Stream>()))
                .ReturnsAsync(new BulkImportStatus { Status = ImportStatus.Error });

            await _importMetricsData.Run(_fileStream, _fileName, _logger, _executionContext);

            _mockUpdateLogHandler.Verify(
                x => x.Handle(It.Is<UpdateLog>(ul => ul.Status == ImportStatus.Error.ToString())),
                Times.Once
            );
        }
    }
}
