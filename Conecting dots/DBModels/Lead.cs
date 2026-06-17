using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Lead
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string LeadName { get; set; } = null!;

    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public int? PropertyId { get; set; }

    public int? ProductId { get; set; }

    public int? CustomerId { get; set; }

    public int LeadSourceId { get; set; }

    public int LeadStatusId { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    public string? Remarks { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();

    public virtual ReferenceCode LeadSource { get; set; } = null!;

    public virtual LeadStatus LeadStatus { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
