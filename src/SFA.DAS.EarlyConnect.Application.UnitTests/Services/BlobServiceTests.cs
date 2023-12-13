using System.Threading.Tasks;
using Azure;
using Moq;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Services
{
    public class BlobServiceTests
    {
        [Test]
        public async Task CopyBlobAsync_ShouldCallCopyBlobAsyncOnBlobContainerClientWrapper()
        {
            string sourceBlobName = "sourceBlob";
            string sourceContainerName = "sourceContainer";
            string destinationContainerName = "destinationContainer";

            var mockResponse = new Mock<Response>();

            var mockBlobContainerClientWrapper = new Mock<IBlobContainerClientWrapper>();

            mockBlobContainerClientWrapper
                .Setup(x => x.CopyBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockResponse.Object);

            var blobService = new BlobService(mockBlobContainerClientWrapper.Object);

            await blobService.CopyBlobAsync(sourceBlobName, sourceContainerName, destinationContainerName);

            mockBlobContainerClientWrapper.Verify(
                   x => x.CopyBlobAsync(sourceBlobName, sourceContainerName, destinationContainerName),
                   Times.Once);
        }

        [Test]
        public async Task CopyBlobAsync_ShouldReturnResponseFromBlobContainerClientWrapper()
        {
            string sourceBlobName = "sourceBlob";
            string sourceContainerName = "sourceContainer";
            string destinationContainerName = "destinationContainer";

            var mockResponse = new Mock<Response>();

            var mockBlobContainerClientWrapper = new Mock<IBlobContainerClientWrapper>();

            mockBlobContainerClientWrapper
                .Setup(x => x.CopyBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockResponse.Object);

            var blobService = new BlobService(mockBlobContainerClientWrapper.Object);

            var response = await blobService.CopyBlobAsync(sourceBlobName, sourceContainerName, destinationContainerName);

            Assert.AreEqual(mockResponse.Object, response);
        }
    }
}
