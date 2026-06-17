using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class FollowUp
{
    public int Id { get; set; }

    public int? LeadId { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Lead? Lead { get; set; }
}
