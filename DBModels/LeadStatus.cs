using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class LeadStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public int? ParentId { get; set; }

    public int? PreviousStatusId { get; set; }

    public bool InputRequried { get; set; }

    public string? InputControlType { get; set; }

    public string? InputNotes { get; set; }

    public virtual ICollection<LeadStatus> InverseParent { get; set; } = new List<LeadStatus>();

    public virtual ICollection<LeadStatus> InversePreviousStatus { get; set; } = new List<LeadStatus>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual LeadStatus? Parent { get; set; }

    public virtual LeadStatus? PreviousStatus { get; set; }
}
