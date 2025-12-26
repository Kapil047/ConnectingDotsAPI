using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.FileService
{
    public interface IFileService
    {
        Task<Download> DeleteDownload(Guid guid);
        Task<Download?> SaveDownload(FileModel.DownloadUploadRequest request);
    }
}