using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class CustomerAuthToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public int CustomerId { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
