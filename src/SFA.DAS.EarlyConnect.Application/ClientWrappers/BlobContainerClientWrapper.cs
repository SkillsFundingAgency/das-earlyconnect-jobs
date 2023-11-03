using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using SFA.DAS.EarlyConnect.Application.Services;

namespace SFA.DAS.EarlyConnect.Application.ClientWrappers
{
    public class BlobContainerClientWrapper : IBlobContainerClientWrapper
    {
        private BlobContainerClient _blobContainerClient;
        private string _connectionString;

        public BlobContainerClientWrapper(string connectionString)
        {
            _connectionString = connectionString;
        }

     

        //public async Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationBlobName, string destinationContainerName)
        //{
        //    var sourceBlobClient = new BlobClient(_connectionString, sourceContainerName, sourceBlobName);
        //    var destinationBlobClient = new BlobClient(_connectionString, destinationContainerName, destinationBlobName);

        //    var sourceBlobUri = sourceBlobClient.Uri;
        //    var destinationBlobUri = destinationBlobClient.Uri;

        //    await destinationBlobClient.StartCopyFromUriAsync(sourceBlobUri);

        //    // Optionally, you can delete the source blob after copying it to the destination container
        //    await sourceBlobClient.DeleteIfExistsAsync();

        //    // Return the copy operation response
        //    // Modify the return type according to your application's needs
        //    return new Response();
        //}

    public Task<Response> DeleteBlobAsync(string blobName, string containerName)
        {
            _blobContainerClient = new BlobContainerClient(_connectionString, containerName);

            return _blobContainerClient.DeleteBlobAsync(blobName);
        }
    }
}