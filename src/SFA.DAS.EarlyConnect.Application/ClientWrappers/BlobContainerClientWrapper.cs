using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper.Configuration;
using CsvHelper;

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

            string destinationBlobName = $"{sourceBlobName}_{uniqueIdentifier}.csv";

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

        public async Task<Response<BlobContentInfo>> UploadToBlob(List<List<KeyValuePair<string, string>>> uploadData, string containerName, string blobName)
        {
            var headers = uploadData.First().Select(pair => pair.Key).ToList();

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(streamWriter, csvConfig))
            {
                // Write header
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write data
                foreach (var rowData in uploadData)
                {
                    foreach (var header in headers)
                    {
                        var pair = rowData.FirstOrDefault(p => p.Key == header);
                        csv.WriteField(pair.Key != null ? pair.Value : string.Empty);
                    }
                    csv.NextRecord();
                }

                streamWriter.Flush();

                // Upload to Azure Blob Storage

                string uniqueIdentifier = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

                string destinationBlobName = $"{blobName}_{uniqueIdentifier}.csv";

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(destinationBlobName);

                memoryStream.Position = 0;

                return await blobClient.UploadAsync(memoryStream, true);
            }
        }
    }

    public interface IBlobContainerClientWrapper
    {
        Task<Response<BlobContentInfo>> UploadToBlob(List<List<KeyValuePair<string, string>>> uploadData,
            string containerName, string blobName);
        Task<Response> CopyBlobAsync(string sourceBlobName, string sourceContainerName, string destinationContainerName);
    }
}