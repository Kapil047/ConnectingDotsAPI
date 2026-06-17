namespace ConnectingDotsAPI.Models
{
    public class FileModel
    {
        public class DownloadUploadRequest
        {
            public string? BlobUrl { get; set; }
            public string? Filename { get; set; }
            public string? EntityName { get; set; }
            public string? EntityGuid { get; set; }
            public int? EntityId { get; set; }
            public string Extension { get; set; } =string.Empty;
            public int? DownloadTypeId { get; set; }
        }
        public class AddFileRequest
        {
            public required string FileUrl { get; set; }
            public required string FileName { get; set; }
        }
        public class FileDetails
        {
            public int? Id { get; set; }
            public required string FileName { get; set; }
            public required string Url { get; set; }
            public required string Blob { get; set; }
        }
    }
}
