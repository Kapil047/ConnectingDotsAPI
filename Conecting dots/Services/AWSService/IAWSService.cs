
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.AwsService
{
    public interface IAWSService
    {
        Task<FileModel.FileDetails> UploadBlob(string fileName, byte[] data, bool regenerateName);
    }
}