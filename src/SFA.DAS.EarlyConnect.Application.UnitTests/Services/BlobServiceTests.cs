using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
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

        [Test]
        public async Task UploadToBlobAsync_ShouldCallUploadToBlobAsyncOnBlobContainerClientWrapper()
        {
            var uploadData = new List<List<KeyValuePair<string, string>>>
            {
                new List<KeyValuePair<string, string>> { new("Key1", "Value1") },
            };

            var containerName = "Container";
            var blobName = "Blob";

            var responseMock = new Mock<Response<BlobContentInfo>>();

            var blobContainerClientWrapperMock = new Mock<IBlobContainerClientWrapper>();
            blobContainerClientWrapperMock
                .Setup(x => x.UploadToBlob(uploadData, containerName, blobName))
                .ReturnsAsync(responseMock.Object);

            var blobService = new BlobService(blobContainerClientWrapperMock.Object);

            var result = await blobService.UploadToBlob(uploadData, containerName, blobName);

            Assert.NotNull(result);

            blobContainerClientWrapperMock.Verify(
                x => x.UploadToBlob(uploadData, containerName, blobName),
                Times.Once);
        }
    }
}
