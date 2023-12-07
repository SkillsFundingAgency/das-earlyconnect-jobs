using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses;

namespace SFA.DAS.EarlyConnect.Infrastructure.UnitTests.OuterApi
{
    [TestFixture]
    public class OuterApiClientTests
    {
        private OuterApiClient _outerApiClient;
        private Mock<IOptions<OuterApiConfiguration>> _configMock;

        [SetUp]
        public void SetUp()
        {
            _configMock = new Mock<IOptions<OuterApiConfiguration>>();
            _configMock.Setup(x => x.Value).Returns(new OuterApiConfiguration { BaseUrl = "https://example.com", Key = "your-api-key" });
        }

        [Test]
        public async Task Get_ValidRequest_ReturnsApiResponse()
        {
            var request = new Mock<IGetApiRequest>();
            request.Setup(x => x.GetUrl).Returns("https://example.com/api");

            var expectedResponse = new ApiResponse<GetMetricsDataByLepsCodeResponse>(
                new GetMetricsDataByLepsCodeResponse(),
                HttpStatusCode.OK,
                string.Empty
            );

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"property\": \"value\" }")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _outerApiClient = new OuterApiClient(httpClient, _configMock.Object);

            var response = await _outerApiClient.Get<GetMetricsDataByLepsCodeResponse>(request.Object);


            Assert.AreEqual(expectedResponse.StatusCode, response.StatusCode);
            Assert.AreEqual(expectedResponse.ErrorContent, response.ErrorContent);
            Assert.NotNull(response.Body);
        }

        [Test]
        public async Task Get_ErrorStatusCode_ReturnsErrorResponse()
        {
            var request = new Mock<IGetApiRequest>();
            request.Setup(x => x.GetUrl).Returns("https://example.com/api");

            var expectedResponse = new ApiResponse<GetMetricsDataByLepsCodeResponse>(
                new GetMetricsDataByLepsCodeResponse(),
                HttpStatusCode.BadRequest,
                "error message"
            );

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("error message")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _outerApiClient = new OuterApiClient(httpClient, _configMock.Object);

            var response = await _outerApiClient.Get<string>(request.Object);

            Assert.AreEqual(expectedResponse.StatusCode, response.StatusCode);
            Assert.AreEqual(expectedResponse.ErrorContent, response.ErrorContent);
        }
    }
}
