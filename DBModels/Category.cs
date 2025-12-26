using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid Guid { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public int DisplayOrder { get; set; }
}
