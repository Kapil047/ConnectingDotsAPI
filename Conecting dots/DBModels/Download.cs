using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Download
{
    public int Id { get; set; }

    public Guid DownloadGuid { get; set; }

    public bool UseDownloadUrl { get; set; }

    public string DownloadUrl { get; set; } = null!;

    public byte[]? DownloadBinary { get; set; }

    public string? ContentType { get; set; }

    public string Filename { get; set; } = null!;

    public string? Extension { get; set; }

    public bool IsNew { get; set; }

    public string? EntityName { get; set; }

    public int? EntityId { get; set; }

    public int? DownloadTypeId { get; set; }
}
