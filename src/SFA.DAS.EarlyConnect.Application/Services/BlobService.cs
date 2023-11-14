using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EarlyConnect.Application.ClientWrappers;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<IBlobService> _logger;
        private IBlobContainerClientWrapper _blobContainerClientWrapper;

        public BlobService(ILogger<IBlobService> logger, IBlobContainerClientWrapper blobContainerClientWrapper)
        {
            _logger = logger;
            _blobContainerClientWrapper = blobContainerClientWrapper;
        }

        public async Task DeleteFile(string uniqueFileIdentifier, string containerName)
        {
            await _blobContainerClientWrapper.DeleteBlobAsync(uniqueFileIdentifier, containerName);
        }
    }
    public interface IBlobService
    {
        Task DeleteFile(string uniqueFileIdentifier, string containerName);
    }
}