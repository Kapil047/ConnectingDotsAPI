using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Announcement
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public string? Type { get; set; }
}
