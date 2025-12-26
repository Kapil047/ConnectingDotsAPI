using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class PagesInRole
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public int RoleId { get; set; }

    public bool Active { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateChanged { get; set; }

    public virtual SitePage Page { get; set; } = null!;

    public virtual UserRole Role { get; set; } = null!;
}
