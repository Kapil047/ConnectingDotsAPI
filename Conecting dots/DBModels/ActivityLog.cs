using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class ActivityLog
{
    public int Id { get; set; }

    public string Comment { get; set; } = null!;

    public string? IpAddress { get; set; }

    public string? EntityName { get; set; }

    public int ActivityLogTypeId { get; set; }

    public int? UserId { get; set; }

    public int? EntityId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ActivityLogType ActivityLogType { get; set; } = null!;

    public virtual User? User { get; set; }
}
