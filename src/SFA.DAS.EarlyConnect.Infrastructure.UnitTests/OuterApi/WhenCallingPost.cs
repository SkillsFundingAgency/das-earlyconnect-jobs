using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;

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
        public async Task Post_ValidRequest_ReturnsApiResponse()
        {
            var request = new Mock<IPostApiRequest>();
            request.Setup(x => x.PostUrl).Returns("https://example.com/api");
            request.Setup(x => x.Data).Returns(new { Property1 = "value1", Property2 = "value2" });

            var expectedResponse = new ApiResponse<string>("response", HttpStatusCode.OK, string.Empty);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("response")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _outerApiClient = new OuterApiClient(httpClient, _configMock.Object);

            var response = await _outerApiClient.Post<string>(request.Object, false);


            Assert.AreEqual(expectedResponse.StatusCode, response.StatusCode);
            Assert.AreEqual(expectedResponse.ErrorContent, response.ErrorContent);
        }

        [Test]
        public async Task Post_ErrorStatusCode_ReturnsErrorResponse()
        {
            var request = new Mock<IPostApiRequest>();
            request.Setup(x => x.PostUrl).Returns("https://example.com/api");
            request.Setup(x => x.Data).Returns(new { Property1 = "value1", Property2 = "value2" });

            var expectedErrorResponse = new ApiResponse<string>(null, HttpStatusCode.BadRequest, "error message");

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

            var response = await _outerApiClient.Post<string>(request.Object, false);

            Assert.AreEqual(expectedErrorResponse.StatusCode, response.StatusCode);
            Assert.AreEqual(expectedErrorResponse.ErrorContent, response.ErrorContent);
        }
    }
}
