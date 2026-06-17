using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class UserRole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? SystemName { get; set; }

    public bool Active { get; set; }

    public bool IsSystemRole { get; set; }

    public bool EnablePasswordLifetime { get; set; }

    public virtual ICollection<PagesInRole> PagesInRoles { get; set; } = new List<PagesInRole>();

    public virtual ICollection<UserRole> ParentRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
