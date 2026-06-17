using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class NewTable
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}
