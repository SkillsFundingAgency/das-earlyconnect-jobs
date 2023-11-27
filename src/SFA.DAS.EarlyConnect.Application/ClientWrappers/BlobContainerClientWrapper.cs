using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SFA.DAS.EarlyConnect.Application.ClientWrappers
{
    public class BlobContainerClientWrapper : IBlobContainerClientWrapper
    {
        private BlobContainerClient _blobContainerClient;
        private readonly string _connectionString;

        public BlobContainerClientWrapper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationContainerName)
        {
            string uniqueIdentifier = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            string destinationBlobName = $"{sourceBlobName}_Copy_{uniqueIdentifier}";

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

            throw new InvalidOperationException($"Blob copy operation from '{sourceBlobName}' to '{destinationBlobName}' was not successful");
        }
    }

    public interface IBlobContainerClientWrapper
    {
        Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationContainerName);
    }
}