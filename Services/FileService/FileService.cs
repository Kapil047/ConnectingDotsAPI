using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;

namespace ConnectingDotsAPI.Services.FileService
{
    public class FileService(ConnectingDotsDbContext db) : IFileService
    {
        private readonly ConnectingDotsDbContext db = db;

        public async Task<Download?> SaveDownload(FileModel.DownloadUploadRequest request)
        {
            if (request.BlobUrl != null && request.EntityName != null)
            {
                if (!request.EntityId.HasValue && !string.IsNullOrEmpty(request.EntityGuid))
                {
                    switch (request.EntityName.Trim().ToLower())
                    {
                        case "customer":
                            {
                                request.EntityId = db.Customers.FirstOrDefault(x => x.Guid == Guid.Parse(request.EntityGuid))?.Id;
                                break;
                            }
                        case "product":
                            {
                                request.EntityId = db.Products.FirstOrDefault(x => x.Guid == Guid.Parse(request.EntityGuid))?.Id;
                                break;
                            }
                        case "category":
                            {
                                request.EntityId = db.ProductCategories.FirstOrDefault(x => x.Guid == Guid.Parse(request.EntityGuid))?.Id;
                                break;
                            }
                    }
                }
                if (!request.EntityId.HasValue) throw new Exception("ENTITY_ID_NOT_FOUND");

                //Checking if same file is getting upload again
                var _download = db.Downloads.Where(x => x.DownloadUrl == request.BlobUrl && x.EntityName == request.EntityName && x.Filename == request.Filename && x.EntityId == request.EntityId).FirstOrDefault();
                if (_download != null) { return _download; }

                await db.Downloads.Where(x=> x.EntityName == request.EntityName && x.EntityId == request.EntityId).ForEachAsync(x=>x.IsNew=false);

                var contentType = string.IsNullOrEmpty(request.Extension) ? await GetContentType(request.BlobUrl) : request.Extension;
                if (contentType != null)
                {
                    var download = new Download
                    {
                        DownloadUrl = request.BlobUrl,
                        EntityId = request.EntityId,
                        IsNew = true,
                        EntityName = request.EntityName,
                        Filename = request.Filename,
                        UseDownloadUrl = true,
                        Extension = string.IsNullOrEmpty(request.Extension) ? Path.GetExtension(request.BlobUrl) : request.Extension,
                        ContentType = contentType,
                        DownloadGuid = Guid.NewGuid(),
                        DownloadTypeId = request.DownloadTypeId
                    };
                    db.Downloads.Add(download);
                    await db.SaveChangesAsync();
                    return download;
                }
            }
            return null;
        }
        public async Task<Download> DeleteDownload(Guid guid)
        {
            var download = db.Downloads.FirstOrDefault(x => x.DownloadGuid == guid) ?? throw new Exception($"{nameof(Download)} does not exist");
            db.Downloads.Remove(download);
            await db.SaveChangesAsync();
            return download;
        }

        private async Task<string> GetContentType(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string contentType = response.Content.Headers.ContentType.MediaType;
                        return contentType;
                    }
                    else
                    {
                        throw new Exception("Failed to retrieve content type. Status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}
