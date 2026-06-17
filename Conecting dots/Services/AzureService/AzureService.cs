using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.CacheService;
using System.IO;

namespace ConnectingDotsAPI.Services.AzureService
{
    public class AzureService(ICacheService cacheService, ConnectingDotsDbContext db) : IAzureService
    {
        private readonly ICacheService cacheService = cacheService;
        private readonly ConnectingDotsDbContext db = db;

        #region Properties

        public string AzureContainerReference()
        {
            var key = "AzureContainerReference";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = FindConfiguration("azure.container");
            cacheService.SetValue(key, value, 60);
            return value;
        }

        public string AzureImageUrl()
        {
            var key = "AzureImageUrl";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = FindConfiguration("azure.url");
            cacheService.SetValue(key, value, 60);
            return value;
        }
        #endregion

        public string GetBlobUrl(string blobReference)
        {
            return $"{AzureImageUrl()}{AzureContainerReference()}/{blobReference}";
        }

        public async Task<FileModel.FileDetails> UploadBlob(byte[] fileContent, string fileName, bool regenerateName)
        {
            var storageConnectionString = AzureStorageConnectionString();

            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerReference().ToLower());

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var reference = regenerateName ? $"{Guid.NewGuid()}|{fileName}" : fileName;
            var blobClient = blobContainerClient.GetBlobClient(reference);

            using var stream = new MemoryStream(fileContent);

            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = GetContentType(fileName) });

            return new FileModel.FileDetails
            {
                Blob = reference,
                FileName = fileName,
                Url = GetBlobUrl(reference)
            };
        }

        public async Task<bool> BlobExistsOnCloud(string blobReference)
        {
            var storageConnectionString = AzureStorageConnectionString();
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerReference().ToLower());

            var blobClient = blobContainerClient.GetBlobClient(blobReference);
            return await blobClient.ExistsAsync();
        }

        public async Task DeleteBlob(string blobReference)
        {
            var storageConnectionString = AzureStorageConnectionString();
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerReference().ToLower());

            var blobClient = blobContainerClient.GetBlobClient(blobReference);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<byte[]> DownloadBlobBytes(string blobReference)
        {
            var storageConnectionString = AzureStorageConnectionString();
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerReference().ToLower());

            var blobClient = blobContainerClient.GetBlobClient(blobReference);

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<string> DownloadBlob(string blobReference)
        {
            var storageConnectionString = AzureStorageConnectionString();
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureContainerReference().ToLower());

            var blobClient = blobContainerClient.GetBlobClient(blobReference);
            var fileName = blobReference.Substring(blobReference.IndexOf("|", StringComparison.Ordinal) + 1);
            var destinationFile = Path.Combine(Path.GetTempPath(), fileName);

            await blobClient.DownloadToAsync(destinationFile);
            return destinationFile;
        }

        private string AzureStorageConnectionString()
        {
            var key = "AzureStorageConnectionString";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = FindConfiguration("azure.connectionstring");
            cacheService.SetValue(key, value, 60);
            return value;
        }

        private string FindConfiguration(string name)
        {
            return db.Settings.First(x => x.Name == name).Value;
        }

        private string GetContentType(string fileName)
        {
            return fileName.ToLower() switch
            {
                var ext when ext.EndsWith(".jpg") || ext.EndsWith(".jpeg") => "image/jpeg",
                var ext when ext.EndsWith(".gif") => "image/gif",
                var ext when ext.EndsWith(".png") => "image/png",
                var ext when ext.EndsWith(".bmp") => "image/bmp",
                var ext when ext.EndsWith(".pdf") => "application/pdf",
                var ext when ext.EndsWith(".doc") || ext.EndsWith(".docx") => "application/vnd.ms-word",
                var ext when ext.EndsWith(".xls") || ext.EndsWith(".xlsx") => "application/vnd.ms-excel",
                _ => "application/octet-stream",
            };
        }
    }
}
