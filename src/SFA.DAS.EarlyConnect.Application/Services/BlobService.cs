using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public class BlobService : IBlobService
    {
        private BlobContainerClient _blobContainerClient;
        private readonly string _connectionString;

        public BlobService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationBlobName, string destinationContainerName)
        {
            var sourceBlobClient = new BlobClient(_connectionString, sourceContainerName, sourceBlobName);
            var destinationBlobClient = new BlobClient(_connectionString, destinationContainerName, destinationBlobName);

            var sourceBlobUri = sourceBlobClient.Uri;
            var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(sourceBlobUri);

            await copyOperation.WaitForCompletionAsync();

            var destinationBlobProperties = await destinationBlobClient.GetPropertiesAsync();

            if (destinationBlobProperties.Value.CopyStatus == CopyStatus.Success)
            {
                _blobContainerClient = new BlobContainerClient(_connectionString, sourceContainerName);
                return await _blobContainerClient.DeleteBlobAsync(sourceBlobName);
            }
            else
            {
                throw new InvalidOperationException($"Blob copy operation from '{sourceBlobName}' to '{destinationBlobName}' was not successful");
            }
        }
    }
    public interface IBlobService
    {
        Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationBlobName,
            string destinationContainerName);
    }
}