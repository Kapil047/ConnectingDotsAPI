using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.AzureService
{
    public interface IAzureService
    {
        string AzureContainerReference();
        string AzureImageUrl();
        string GetBlobUrl(string blobReference);
        Task<FileModel.FileDetails> UploadBlob(byte[] fileContent, string fileName, bool regenerateName);
    }
}