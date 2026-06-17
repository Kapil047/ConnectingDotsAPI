using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Setting
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
