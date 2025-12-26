using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class ProductType
{
    public int Id { get; set; }

    public string SystemKeyword { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
