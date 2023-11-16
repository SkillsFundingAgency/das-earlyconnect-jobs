using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Handlers;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests;
using SFA.DAS.EarlyConnect.Models.BulkImport;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Handlers
{

    [TestFixture]
    public class WhenHandlingImportMetricsDataRequest
    {
        private MetricsDataBulkUploadHandler _handler;
        private Mock<ILogger<MetricsDataBulkUploadHandler>> _logger;
        private Mock<IOuterApiClient> _outerApiClient;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<MetricsDataBulkUploadHandler>>();
            _outerApiClient = new Mock<IOuterApiClient>();
            _handler = new MetricsDataBulkUploadHandler(_logger.Object, _outerApiClient.Object, new CsvService());
        }

        [Test]
        public async Task Handle_ValidCsvFile_ReturnsCompletedStatus()
        {
            var loggerMock = new Mock<ILogger<MetricsDataBulkUploadHandler>>();
            var csvServiceMock = new Mock<ICsvService>();


            var handler =
                new MetricsDataBulkUploadHandler(loggerMock.Object, _outerApiClient.Object, csvServiceMock.Object);

            var csvData =
                "Region,Intended_uni_entry_year,Max_travel_distance,Willing_to_relocate_flag,Number_gcse_grade4,Students,Interested_in_transport_flag";

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(csvData);
                writer.Flush();
                stream.Position = 0;

                csvServiceMock.Setup(x => x.ConvertToList(It.IsAny<StreamReader>()))
                    .ReturnsAsync(new List<dynamic>
                    {
                        new Dictionary<string, object>
                        {
                            { "Region", "North" },
                            { "Intended_uni_entry_year", "2022" },
                            { "Max_travel_distance", "0_10_miles" },
                            { "Willing_to_relocate_flag", "Y" },
                            { "Number_gcse_grade4", "8" },
                            { "Students", "100" },
                            { "Interested_in_transport_flag", "1" }
                        }
                    });

                _outerApiClient.Setup(c => c.Post<object>(It.IsAny<CreateMetricsDataRequest>(), false))
                    .ReturnsAsync(new ApiResponse<object>(new object(), HttpStatusCode.OK, string.Empty));

                var result = await handler.Handle(stream);

                Assert.AreEqual(ImportStatus.Completed, result.Status);
            }
        }

        [Test]
        public async Task Handle_ExternalApiFails_ReturnsFailedStatus()
        {
            var loggerMock = new Mock<ILogger<MetricsDataBulkUploadHandler>>();
            var csvServiceMock = new Mock<ICsvService>();

            var handler = new MetricsDataBulkUploadHandler(loggerMock.Object, _outerApiClient.Object, csvServiceMock.Object);

            var csvData =
                "Region,Intended_uni_entry_year,Max_travel_distance,Willing_to_relocate_flag,Number_gcse_grade4,Students,Interested_in_transport_flag";

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(csvData);
                writer.Flush();
                stream.Position = 0;

                csvServiceMock.Setup(x => x.ConvertToList(It.IsAny<StreamReader>()))
                    .ReturnsAsync(new List<dynamic>
                    {
                        new Dictionary<string, object>
                        {
                            { "Region", "North" },
                            { "Intended_uni_entry_year", "2022" },
                            { "Max_travel_distance", "0_10_miles" },
                            { "Willing_to_relocate_flag", "Y" },
                            { "Number_gcse_grade4", "8" },
                            { "Students", "100" },
                            { "Interested_in_transport_flag", "1" }
                        }
                    });

                _outerApiClient.Setup(c => c.Post<object>(It.IsAny<CreateMetricsDataRequest>(), false))
                                .ReturnsAsync(new ApiResponse<object>(null, HttpStatusCode.InternalServerError, "Simulated API failure"));

                var result = await handler.Handle(stream);

                Assert.AreEqual(ImportStatus.Error, result.Status);
            }
        }

    }
}