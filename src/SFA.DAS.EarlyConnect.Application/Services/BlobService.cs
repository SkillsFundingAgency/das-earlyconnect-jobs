using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;
using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public class BlobService : IBlobService
    {
        private IBlobContainerClientWrapper _blobContainerClientWrapper;

        public BlobService(IBlobContainerClientWrapper blobContainerClientWrapper)
        {
            _blobContainerClientWrapper = blobContainerClientWrapper;
        }

        public async Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationContainerName)
        {
            return await _blobContainerClientWrapper.CopyBlobAsync(sourceBlobName, sourceContainerName,
                destinationContainerName);
        }

        public async Task<Response<BlobContentInfo>> UploadToBlob(List<List<KeyValuePair<string, string>>> uploadData, string containerName, string blobName)
        {
            return await _blobContainerClientWrapper.UploadToBlob(uploadData, containerName, blobName);
        }
    }
    public interface IBlobService
    {
        Task<Response<BlobContentInfo>> UploadToBlob(List<List<KeyValuePair<string, string>>> uploadData,
            string containerName, string blobName);

        Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName,
            string destinationContainerName);
    }
}
