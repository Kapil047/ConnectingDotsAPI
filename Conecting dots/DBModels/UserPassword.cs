using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class UserPassword
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Password { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public virtual User User { get; set; } = null!;
}
