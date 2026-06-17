using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    public Guid OrderItemGuid { get; set; }

    public int Quantity { get; set; }

    public string? AttributeDescription { get; set; }

    public string? Style { get; set; }

    public string? Color { get; set; }

    public string? Uom { get; set; }

    public string? Weight { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
