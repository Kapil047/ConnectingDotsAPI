using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Username { get; set; } = null!;

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public Guid Guid { get; set; }

    public int? ParentUserId { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<Traceability> TraceabilityManagerUsers { get; set; } = new List<Traceability>();

    public virtual ICollection<Traceability> TraceabilitySuppliers { get; set; } = new List<Traceability>();

    public virtual ICollection<UserAuthToken> UserAuthTokens { get; set; } = new List<UserAuthToken>();

    public virtual ICollection<UserPassword> UserPasswords { get; set; } = new List<UserPassword>();

    public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}
