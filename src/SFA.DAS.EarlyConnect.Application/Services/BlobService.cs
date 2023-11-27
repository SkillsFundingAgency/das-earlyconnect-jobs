using System.Threading.Tasks;
using Azure;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;

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
    }
    public interface IBlobService
    {
        Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName,
            string destinationContainerName);
    }
}
