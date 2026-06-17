using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class SitePage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateChanged { get; set; }

    public string SystemName { get; set; } = null!;

    public virtual ICollection<PagesInRole> PagesInRoles { get; set; } = new List<PagesInRole>();
}
