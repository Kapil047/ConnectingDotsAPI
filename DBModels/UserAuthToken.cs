using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class UserAuthToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
