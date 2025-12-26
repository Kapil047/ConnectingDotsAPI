using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class CustomerPassword
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? Password { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
